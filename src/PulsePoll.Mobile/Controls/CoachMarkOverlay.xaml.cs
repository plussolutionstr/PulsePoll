using Microsoft.Maui.Graphics;

namespace PulsePoll.Mobile.Controls;

public class CoachMarkStep
{
    public required View Target { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public float CornerRadius { get; init; } = 12f;
    public int Padding { get; init; } = 8;
}

public partial class CoachMarkOverlay : ContentView
{
    private const string PreferencePrefix = "coachmark_";

    private readonly SpotlightDrawable _drawable = new();
    private List<CoachMarkStep> _steps = [];
    private int _currentIndex;
    private string _key = "default";

    public event EventHandler? Completed;
    public event EventHandler? Skipped;

    public string Key
    {
        get => _key;
        set => _key = value;
    }

    public CoachMarkOverlay()
    {
        InitializeComponent();
        SpotlightView.Drawable = _drawable;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => { /* swallow taps on backdrop */ };
        SpotlightView.GestureRecognizers.Add(tap);
    }

    public bool HasBeenShown => Preferences.Default.Get(PreferencePrefix + _key, false);

    public static bool IsCompleted(string key) => Preferences.Default.Get(PreferencePrefix + key, false);

    public static void Reset(string key) => Preferences.Default.Remove(PreferencePrefix + key);

    public static void ResetAll()
    {
        foreach (var k in new[] { "home", "wallet", "profile" })
            Preferences.Default.Remove(PreferencePrefix + k);
    }

    public async Task ShowAsync(List<CoachMarkStep> steps)
    {
        if (steps.Count == 0)
            return;

        _steps = steps;
        _currentIndex = 0;
        IsVisible = true;
        InputTransparent = false;

        await ShowStepAsync();
    }

    private async Task ShowStepAsync()
    {
        var step = _steps[_currentIndex];

        StepIndicator.Text = $"{_currentIndex + 1}/{_steps.Count}";
        TooltipTitle.Text = step.Title;
        TooltipDescription.Text = step.Description;
        NextButton.Text = _currentIndex == _steps.Count - 1 ? "Anladım" : "İleri";
        SkipButton.IsVisible = _currentIndex < _steps.Count - 1;

        await PositionElementsAsync(step);
    }

    private async Task PositionElementsAsync(CoachMarkStep step)
    {
        TooltipBubble.Opacity = 0;
        _drawable.Opacity = 0;
        SpotlightView.Invalidate();

        if (RootGrid.Width <= 0)
            await Task.Delay(50);

        var targetBounds = GetTargetBounds(step.Target);
        if (targetBounds == Rect.Zero)
        {
            await Task.Delay(100);
            targetBounds = GetTargetBounds(step.Target);
        }

        var pad = step.Padding;
        var spotX = targetBounds.X - pad;
        var spotY = targetBounds.Y - pad;
        var spotW = targetBounds.Width + pad * 2;
        var spotH = targetBounds.Height + pad * 2;

        var pageW = RootGrid.Width;
        var pageH = RootGrid.Height;

        // Update drawable and invalidate
        _drawable.SpotlightRect = new RectF((float)spotX, (float)spotY, (float)spotW, (float)spotH);
        _drawable.CornerRadius = step.CornerRadius;
        _drawable.Opacity = 1;
        SpotlightView.Invalidate();

        // Position tooltip — prefer below, fallback above
        var tooltipMarginH = 20.0;
        var tooltipMaxW = Math.Min(300, pageW - tooltipMarginH * 2);
        TooltipBubble.MaximumWidthRequest = tooltipMaxW;

        var spaceBelow = pageH - (spotY + spotH);
        var spaceAbove = spotY;
        var tooltipEstimatedH = 140.0;

        if (spaceBelow > tooltipEstimatedH + 16)
        {
            TooltipBubble.Margin = new Thickness(tooltipMarginH, spotY + spotH + 12, tooltipMarginH, 0);
            TooltipBubble.VerticalOptions = LayoutOptions.Start;
        }
        else if (spaceAbove > tooltipEstimatedH + 16)
        {
            TooltipBubble.Margin = new Thickness(tooltipMarginH, 0, tooltipMarginH, pageH - spotY + 12);
            TooltipBubble.VerticalOptions = LayoutOptions.End;
        }
        else
        {
            TooltipBubble.Margin = new Thickness(tooltipMarginH, 0, tooltipMarginH, 0);
            TooltipBubble.VerticalOptions = LayoutOptions.Center;
        }

        TooltipBubble.HorizontalOptions = LayoutOptions.Fill;

        // Animate in
        await TooltipBubble.FadeToAsync(1, 250, Easing.CubicOut);
    }

    private Rect GetTargetBounds(View target)
    {
        var x = 0.0;
        var y = 0.0;

        View? current = target;
        while (current is not null && current != RootGrid.Parent)
        {
            x += current.X;
            y += current.Y;

            if (current.Parent is View parentView)
                current = parentView;
            else
                break;
        }

        View? overlayParent = this;
        var ox = 0.0;
        var oy = 0.0;
        while (overlayParent is not null && overlayParent != RootGrid.Parent?.Parent)
        {
            ox += overlayParent.X;
            oy += overlayParent.Y;
            if (overlayParent.Parent is View pv)
                overlayParent = pv;
            else
                break;
        }

        x -= ox;
        y -= oy;

        return new Rect(x, y, target.Width, target.Height);
    }

    private void OnNextClicked(object? sender, EventArgs e)
    {
        _currentIndex++;
        if (_currentIndex >= _steps.Count)
        {
            MarkCompleted();
            Dismiss();
            Completed?.Invoke(this, EventArgs.Empty);
            return;
        }

        _ = TransitionToStepAsync();
    }

    private void OnSkipClicked(object? sender, EventArgs e)
    {
        MarkCompleted();
        Dismiss();
        Skipped?.Invoke(this, EventArgs.Empty);
    }

    private async Task TransitionToStepAsync()
    {
        await TooltipBubble.FadeToAsync(0, 150);
        await ShowStepAsync();
    }

    private void Dismiss()
    {
        TooltipBubble.Opacity = 0;
        _drawable.Opacity = 0;
        SpotlightView.Invalidate();
        IsVisible = false;
        InputTransparent = true;
    }

    private void MarkCompleted()
    {
        Preferences.Default.Set(PreferencePrefix + _key, true);
    }
}

public class SpotlightDrawable : IDrawable
{
    public RectF SpotlightRect { get; set; }
    public float CornerRadius { get; set; } = 12f;
    public float Opacity { get; set; } = 1f;

    private static readonly Color OverlayColor = Color.FromArgb("#CC1A1535");
    private static readonly Color RingColor = Color.FromArgb("#7C5CFC");
    private const float RingThickness = 3f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Opacity <= 0)
            return;

        // --- 1. Draw overlay with rounded cutout ---
        var path = new PathF();

        // Outer rectangle (full screen)
        path.MoveTo(0, 0);
        path.LineTo(dirtyRect.Width, 0);
        path.LineTo(dirtyRect.Width, dirtyRect.Height);
        path.LineTo(0, dirtyRect.Height);
        path.Close();

        // Inner rounded rectangle (spotlight hole)
        AppendRoundedRect(path, SpotlightRect, CornerRadius);

        canvas.SetFillPaint(new SolidPaint(OverlayColor), dirtyRect);
        canvas.FillPath(path, WindingMode.EvenOdd);

        // --- 2. Draw highlight ring around cutout ---
        var ringPath = new PathF();
        AppendRoundedRect(ringPath, SpotlightRect, CornerRadius);

        canvas.StrokeColor = RingColor;
        canvas.StrokeSize = RingThickness;
        canvas.DrawPath(ringPath);
    }

    private static void AppendRoundedRect(PathF path, RectF rect, float r)
    {
        var x = rect.X;
        var y = rect.Y;
        var w = rect.Width;
        var h = rect.Height;

        path.MoveTo(x + r, y);
        path.LineTo(x + w - r, y);
        path.QuadTo(x + w, y, x + w, y + r);
        path.LineTo(x + w, y + h - r);
        path.QuadTo(x + w, y + h, x + w - r, y + h);
        path.LineTo(x + r, y + h);
        path.QuadTo(x, y + h, x, y + h - r);
        path.LineTo(x, y + r);
        path.QuadTo(x, y, x + r, y);
        path.Close();
    }
}
