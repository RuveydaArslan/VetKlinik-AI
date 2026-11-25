// Sayfa ortasındaki bildirim kutucuklarını (Views/Shared/_Bildirim.cshtml) göster.
// Başarı mesajı 3 sn, hata mesajı 6 sn sonra kendiliğinden kapanır (data-bs-delay); X ile de kapatılabilir.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".bildirim-toast").forEach(function (kutu) {
        bootstrap.Toast.getOrCreateInstance(kutu).show();
    });
});
