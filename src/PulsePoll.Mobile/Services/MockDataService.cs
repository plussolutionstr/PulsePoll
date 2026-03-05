using PulsePoll.Mobile.Models;

namespace PulsePoll.Mobile.Services;

public class MockDataService
{
    public List<StoryModel> GetStories() =>
    [
        new(1, "PulsePoll'a Hoş Geldin", "icon_star.png", LinkUrl: "https://pulsepoll.com", Description: "Yeni görevler ve fırsatları kaçırma.", BrandColor: "#7C5CFC", IsSeen: false),
        new(2, "Arçelik Kampanyası", "icon_star.png", Description: "Ev aletleri kullanıcı deneyimini paylaş.", BrandColor: "#E04A2F"),
        new(3, "Turkcell Fırsatı", "icon_star.png", Description: "Hızlı anketle ekstra puan kazan.", BrandColor: "#0060AF"),
        new(4, "Migros Alışveriş Günü", "icon_star.png", Description: "Market alışkanlıklarını anlat, ödül kazan.", BrandColor: "#007A33", IsSeen: true),
        new(5, "BMW Deneyim Anketi", "icon_star.png", Description: "Sürüş alışkanlıklarını bizimle paylaş.", BrandColor: "#1F4E79", IsSeen: true),
        new(6, "Akbank Fintech Story", "icon_star.png", Description: "Bankacılık uygulama deneyimini değerlendir.", BrandColor: "#7C5CFC", IsSeen: true)
    ];

    public List<NewsModel> GetNews() =>
    [
        new(1, "Duyuru", "Yeni Platin Üyelik Sistemi Geliyor", "Daha fazla kazanç, daha fazla fırsat!",
            "#2D1B6E", "#A78BFA", LinkUrl: "https://pulsepoll.com"),
        new(2, "Kampanya", "Hafta Sonu Anketi 2x Puan", "Cumartesi-Pazar tamamlanan anketler çift puan!",
            "#1F4E79", "#3B82F6"),
        new(3, "Yenilik", "Yeni Kategori: Otomotiv", "Otomotiv anketleri ile daha fazla kazanın.",
            "#0B4D2C", "#22C988")
    ];

    public List<SurveyModel> GetSurveys() =>
    [
        new(1, "Arçelik", "arcelik", "Ev Aletleri Kullanım Anketi",
            "Arçelik ev aletleri hakkındaki deneyimlerinizi ve tercihlerinizi öğrenmek istiyoruz.",
            "Teknoloji", 15m, 8, 12,
            [
                new("18-45 yaş arası", true),
                new("İstanbul, Ankara veya İzmir", true),
                new("Son 1 yılda ev aleti satın almış", false)
            ],
            BannerGradientStart: "#FFF1EE", BannerGradientEnd: "#FFE4DC", BrandColor: "#E04A2F"),
        new(2, "Turkcell", "turkcell", "Mobil Uygulama Deneyimi",
            "Turkcell mobil uygulama kullanım alışkanlıklarınızı değerlendirin.",
            "Telekomünikasyon", 22m, 10, 15,
            [
                new("Turkcell abonesi", true),
                new("Akıllı telefon kullanıcısı", true),
                new("18 yaş üstü", true)
            ],
            BannerGradientStart: "#EEF4FF", BannerGradientEnd: "#DBEAFE", BrandColor: "#0060AF"),
        new(3, "Migros", "migros", "Alışveriş Alışkanlıkları",
            "Haftalık market alışverişi tercihlerinizi paylaşın.",
            "Perakende", 18m, 7, 10,
            [
                new("Düzenli market alışverişi yapan", true),
                new("25-55 yaş arası", true),
                new("Migros kartı sahibi", false)
            ],
            BannerGradientStart: "#EDFDF5", BannerGradientEnd: "#D1FAE5", BrandColor: "#007A33")
    ];

    public SurveyModel GetSurveyDetail(int id) =>
        GetSurveys().FirstOrDefault(s => s.Id == id) ?? GetSurveys()[0];

    public List<QuestionModel> GetQuestions() =>
    [
        new(1, "Evde en sık kullandığınız Arçelik ürünü hangisi?",
            [
                new(1, "Bulaşık makinesi"),
                new(2, "Çamaşır makinesi"),
                new(3, "Buzdolabı"),
                new(4, "Fırın"),
                new(5, "Elektrikli süpürge")
            ]),
        new(2, "Ürünlerimizi ne sıklıkla kullanıyorsunuz?",
            [
                new(1, "Her gün"),
                new(2, "Haftada birkaç kez"),
                new(3, "Haftada bir"),
                new(4, "Ayda birkaç kez"),
                new(5, "Nadiren")
            ]),
        new(3, "Ürün kalitesini nasıl değerlendirirsiniz?",
            [
                new(1, "Çok iyi"),
                new(2, "İyi"),
                new(3, "Orta"),
                new(4, "Kötü"),
                new(5, "Çok kötü")
            ])
    ];

    public ProfileModel GetProfile() => new(
        "Ahmet Yılmaz",
        "ahmet.yilmaz@email.com",
        "avatar",
        "Gold",
        4250,
        48,
        3,
        94,
        [
            new("Yaş", "28"),
            new("Cinsiyet", "Erkek"),
            new("Eğitim", "Lisans"),
            new("Medeni Durum", "Bekar"),
            new("Konum", "İstanbul, Kadıköy"),
            new("Meslek", "Yazılım Geliştirici"),
            new("Gelir", "25.000 - 35.000 ₺")
        ],
        ["Teknoloji", "Otomotiv", "Fintech", "Yazılım", "Yatırım"]);

    public List<HistoryGroup> GetHistory() =>
    [
        new("Mart 2026",
        [
            new(1, "Ev Aletleri Kullanım Anketi", "Arçelik", "Tamamlandı", 15m,
                new DateTime(2026, 3, 2), 12),
            new(2, "Mobil Uygulama Deneyimi", "Turkcell", "Devam Ediyor", null,
                new DateTime(2026, 3, 1), 15)
        ]),
        new("Şubat 2026",
        [
            new(3, "Alışveriş Alışkanlıkları", "Migros", "Tamamlandı", 18m,
                new DateTime(2026, 2, 25), 10),
            new(4, "Sürüş Deneyimi Anketi", "BMW", "Elendi", null,
                new DateTime(2026, 2, 20), 8),
            new(5, "Bankacılık Hizmetleri", "Akbank", "Tamamlandı", 20m,
                new DateTime(2026, 2, 15), 14)
        ])
    ];

    public WalletModel GetWallet() => new(
        215.00m,
        4250,
        380.00m,
        [
            new(1, "Akbank", "TR** **** **** **** **** 4521"),
            new(2, "Garanti BBVA", "TR** **** **** **** **** 7834")
        ],
        [
            new(1, "Ev Aletleri Anketi", "Arçelik", 15m, new DateTime(2026, 3, 2), true),
            new(2, "Para Çekme", "Akbank hesabına", -100m, new DateTime(2026, 2, 28), false),
            new(3, "Alışveriş Anketi", "Migros", 18m, new DateTime(2026, 2, 25), true),
            new(4, "Bankacılık Anketi", "Akbank", 20m, new DateTime(2026, 2, 15), true)
        ]);

    public List<NotificationModel> GetNotifications() =>
    [
        new(1, "survey", "Yeni Anket", "Arçelik ev aletleri anketi seni bekliyor!",
            DateTime.Now.AddHours(-1), false),
        new(2, "earning", "Kazanç Onaylandı", "Turkcell anketi için ₺22 hesabına yatırıldı.",
            DateTime.Now.AddHours(-3), false),
        new(3, "rank", "Seviye Atladın!", "Tebrikler! Gold seviyeye ulaştın.",
            DateTime.Now.AddDays(-1), true),
        new(4, "disqualified", "Anket Sonlandı", "BMW sürüş anketi için uygun değilsiniz.",
            DateTime.Now.AddDays(-1), true),
        new(5, "system", "Hatırlatma", "Tamamlanmamış bir anketin var. Son 2 gün!",
            DateTime.Now.AddDays(-3), true)
    ];
}
