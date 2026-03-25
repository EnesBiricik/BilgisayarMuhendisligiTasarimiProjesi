function initializeThemeSettings() {
    // Sayfa yüklendiğinde kaydedilmiş temayı uygula
    const savedTheme = localStorage.getItem('theme') || 'light';
    applyTheme(savedTheme);

    // Layout ayarları
    change_box_container('false');
    layout_caption_change('true');
    layout_rtl_change('false');
    preset_change('preset-1');
    main_layout_change('vertical');
    localStorage.setItem('layout', 'vertical');
}

// Sayfa yüklendiğinde tema ayarlarını başlat
document.addEventListener('DOMContentLoaded', initializeThemeSettings); 