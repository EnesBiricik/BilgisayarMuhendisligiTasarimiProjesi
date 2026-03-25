// Menü linklerini ve sayfaları tutan array
const menuLinks = [
    // Genel
    { title: 'Anasayfa', url: '/panel', keywords: ['ana sayfa', 'dashboard', 'panel'] },
    
    // Hizmetler
    { title: 'Hizmetler Listesi', url: '/Service/List', keywords: ['hizmet', 'hizmetler', 'servis'] },
    { title: 'Hizmet Ekle', url: '/Service/Create', keywords: ['hizmet ekle', 'yeni hizmet'] },
    
    // Kategoriler
    { title: 'Kategoriler', url: '/Category/List', keywords: ['kategori', 'kategoriler'] },
    { title: 'Kategori Ekle', url: '/Category/Create', keywords: ['kategori ekle', 'yeni kategori'] },
    
    // Mesajlar
    { title: 'Mesajlar', url: '/Message/List', keywords: ['mesaj', 'mesajlar', 'iletişim'] },
    
    // Referanslar
    { title: 'Referans Listesi', url: '/Reference/List', keywords: ['referans', 'referanslar'] },
    { title: 'Referans Ekle', url: '/Reference/Create', keywords: ['referans ekle', 'yeni referans'] },
    
    // Slider
    { title: 'Slider Listesi', url: '/Slider/List', keywords: ['slider', 'slayt'] },
    { title: 'Slider Ekle', url: '/Slider/Create', keywords: ['slider ekle', 'yeni slider'] },
    
    // Sosyal Medya
    { title: 'Sosyal Medya Listesi', url: '/SocialMedia/List', keywords: ['sosyal', 'medya', 'sosyal medya'] },
    { title: 'Sosyal Medya Ekle', url: '/SocialMedia/Create', keywords: ['sosyal medya ekle', 'yeni sosyal medya'] },
    
    // Ürünler
    { title: 'Ürün Listesi', url: '/Product/List', keywords: ['ürün', 'ürünler', 'urun'] },
    { title: 'Ürün Ekle', url: '/Product/Create', keywords: ['ürün ekle', 'yeni ürün'] },
    
    // Ayarlar
    { title: 'Site Ayarları', url: '/Settings/UpdateSettings', keywords: ['ayar', 'ayarlar', 'site ayarları'] },
    { title: 'SEO Ayarları', url: '/Seo/Edit/1', keywords: ['seo', 'arama motoru'] },
    
    // Hakkımızda
    { title: 'Hakkımızda Listesi', url: '/About/List', keywords: ['hakkımızda', 'hakkinda'] },
    { title: 'Hakkımızda Ekle', url: '/About/Create', keywords: ['hakkımızda ekle', 'yeni hakkımızda'] },
    
    // Kullanıcılar
    { title: 'Kullanıcı Listesi', url: '/User/List', keywords: ['kullanıcı', 'kullanıcılar', 'user'] },
    { title: 'Kullanıcı Ekle', url: '/User/Create', keywords: ['kullanıcı ekle', 'yeni kullanıcı'] },
    { title: 'Hesap Ayarları', url: '/User/AccountSettings', keywords: ['hesap', 'profil', 'account'] }
];

// Arama işlevselliği
function initializeSearch() {
    let searchTimeout;
    const searchInput = document.getElementById('quickSearch');
    const searchResults = document.getElementById('searchResults');

    if (searchInput && searchResults) {
        // Başlangıçta dropdown'ı gizle
        searchResults.style.display = 'none';

        searchInput.addEventListener('input', function(e) {
            clearTimeout(searchTimeout);
            const searchTerm = e.target.value.toLowerCase().trim();
            
            // Input boşsa dropdown'ı gizle
            if (searchTerm.length === 0) {
                searchResults.style.display = 'none';
                searchResults.classList.remove('show');
                return;
            }

            searchTimeout = setTimeout(() => {
                const filteredLinks = menuLinks.filter(link => {
                    return link.title.toLowerCase().includes(searchTerm) || 
                           link.keywords.some(keyword => keyword.toLowerCase().includes(searchTerm));
                });

                if (filteredLinks.length > 0) {
                    searchResults.innerHTML = filteredLinks.map(link => `
                        <li><a class="dropdown-item" href="${link.url}">${link.title}</a></li>
                    `).join('');
                    searchResults.style.display = 'block';
                    searchResults.classList.add('show');
                } else {
                    searchResults.innerHTML = '<li><span class="dropdown-item-text">Sonuç bulunamadı</span></li>';
                    searchResults.style.display = 'block';
                    searchResults.classList.add('show');
                }
            }, 200);
        });

        // Input'a focus olduğunda ve değer varsa sonuçları göster
        searchInput.addEventListener('focus', function() {
            const searchTerm = this.value.trim();
            if (searchTerm.length > 0) {
                searchResults.style.display = 'block';
                searchResults.classList.add('show');
            }
        });

        // Dışarı tıklandığında kapat
        document.addEventListener('click', function(e) {
            if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
                searchResults.style.display = 'none';
                searchResults.classList.remove('show');
            }
        });

        // Input temizlendiğinde dropdown'ı gizle
        searchInput.addEventListener('search', function(e) {
            if (this.value.trim().length === 0) {
                searchResults.style.display = 'none';
                searchResults.classList.remove('show');
            }
        });
    }
}

// Sayfa yüklendiğinde arama işlevselliğini başlat
document.addEventListener('DOMContentLoaded', initializeSearch); 