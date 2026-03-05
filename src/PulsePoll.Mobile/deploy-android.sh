#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_FILE="$PROJECT_DIR/PulsePoll.Mobile.csproj"

TFM="${TFM:-net10.0-android}"
CONFIGURATION="${CONFIGURATION:-Debug}"
RID="${RID:-android-arm64}"
APP_ID="${APP_ID:-com.companyname.pulsepoll.mobile}"
ADB_SERIAL="${ADB_SERIAL:-}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet bulunamadi."
  exit 1
fi

if ! command -v adb >/dev/null 2>&1; then
  echo "adb bulunamadi. Android SDK platform-tools PATH'e eklenmeli."
  exit 1
fi

if [[ -n "$ADB_SERIAL" ]]; then
  DEVICE_STATE="$(adb -s "$ADB_SERIAL" get-state 2>/dev/null || true)"
else
  DEVICE_STATE="$(adb get-state 2>/dev/null || true)"
fi
if [[ "$DEVICE_STATE" != "device" ]]; then
  echo "Hazir Android cihaz bulunamadi."
  echo "Kontrol: adb devices -l"
  echo "Gerekirse seri no ver: ADB_SERIAL=<serial> ./deploy-android.sh"
  exit 1
fi

echo "Build aliniyor..."
dotnet build "$PROJECT_FILE" \
  -f "$TFM" \
  -c "$CONFIGURATION" \
  -p:RuntimeIdentifier="$RID" \
  -v minimal

APK_PATH="$PROJECT_DIR/bin/$CONFIGURATION/$TFM/$RID/$APP_ID-Signed.apk"
if [[ ! -f "$APK_PATH" ]]; then
  APK_PATH="$PROJECT_DIR/bin/$CONFIGURATION/$TFM/$RID/$APP_ID.apk"
fi

if [[ ! -f "$APK_PATH" ]]; then
  echo "APK bulunamadi. Beklenen konum:"
  echo "  $PROJECT_DIR/bin/$CONFIGURATION/$TFM/$RID/"
  exit 1
fi

echo "Cihaza yukleniyor: $APK_PATH"
if [[ -n "$ADB_SERIAL" ]]; then
  adb -s "$ADB_SERIAL" install -r "$APK_PATH"
else
  adb install -r "$APK_PATH"
fi

echo "Deploy tamam."
