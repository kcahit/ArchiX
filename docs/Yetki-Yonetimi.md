# WebHost.Prod Dashboard ve Dinamik Menü Tasarımı

Bu doküman, `ArchiX.WebHost` için **rol tabanlı**, **dinamik** (DB veya class üzerinden `foreach` ile üretilen) dashboard ve sol menü yapısının tasarımını açıklar.

---

## 1. Temel Hedefler

- Menü ve dashboard tamamen dinamik olacak.
- Sol menü elemanları **DB tabanlı** olacak (isterseniz başlangıçta in‑memory list veya `appsettings` üzerinden de beslenebilir).
- `IsAdmin == true` kullanıcı **tüm menüleri ve tüm CRUD yetkilerini** görür.
- Diğer kullanıcılar; **User → Role → MenuPermission** zinciriyle yetkilendirilir.
- Razor Pages tarafında her şey `foreach` ile render edilir, sabit HTML yok.

---

## 2. Domain Class Tasarımları

### 2.1. User

```csharp
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? FullName { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}
```

### 2.2. Role

```csharp
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }   // "Admin", "User", "Sales" vb.
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
    public ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
}
```

### 2.3. UserRole (N‑N Köprü)

```csharp
public class UserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    // Navigation
    public User User { get; set; }
    public Role Role { get; set; }
}
```

### 2.4. Menu (Sol Menü ve Üst Menü İçin Ortak)

```csharp
public class Menu
{
    public int Id { get; set; }

    // Sistem içi name (sabit), başlık değişebilir
    public string Name { get; set; }          // "Dashboard", "Users", "Orders" vb.

    // Ekranda gözüken metin
    public string Title { get; set; }         // "Gösterge Paneli", "Kullanıcılar" vb.

    // Razor Pages için url
    public string? Url { get; set; }          // "/Dashboard", "/Admin/Users" vb.

    // UI
    public string? IconCss { get; set; }      // "bi-speedometer2", "bi-people" vb.

    // Hiyerarşi
    public int? ParentId { get; set; }
    public int Order { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Menu? Parent { get; set; }
    public ICollection<Menu> Children { get; set; } = new List<Menu>();
    public ICollection<MenuPermission> Permissions { get; set; } = new List<MenuPermission>();
}
```

### 2.5. MenuPermission (Önerilen: 4 Ayrı CRUD Kolonu)

```csharp
public class MenuPermission
{
    public int Id { get; set; }

    public int RoleId { get; set; }
    public int MenuId { get; set; }

    // CRUD Yetkileri
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Role Role { get; set; }
    public Menu Menu { get; set; }
}
```

> Not: İsterseniz alternatif olarak `string Crud = "CRU"` şeklinde de tutulabilir; ama okunabilirlik ve query kolaylığı için 4 kolonlu yapı tercih edilir.

---

## 3. DTO Tasarımı (UI / Razor Pages İçin)

UI tarafında daha temiz model kullanmak için domain classlarının üstüne DTO katmanı konulabilir.

### 3.1. MenuDto (Razor için)

```csharp
public class MenuDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string? Url { get; set; }
    public string? IconCss { get; set; }

    public int? ParentId { get; set; }
    public int Order { get; set; }

    // Yetkiler (kullanıcıya göre hesaplanmış)
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }

    public List<MenuDto> Children { get; set; } = new();
}
```

### 3.2. UserMenuModel (Dashboard Layout İçin)

```csharp
public class UserMenuModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public bool IsAdmin { get; set; }

    public List<MenuDto> Menus { get; set; } = new();
}
```

---

## 4. Servis Arayüzleri

### 4.1. IMenuService

```csharp
public interface IMenuService
{
    // Kullanıcının görebileceği menü ağacını döndürür
    Task<UserMenuModel> GetUserMenuAsync(int userId);

    // Yönetim ekranları için
    Task<IReadOnlyList<Menu>> GetAllAsync();
    Task<Menu> GetByIdAsync(int id);
    Task<Menu> CreateAsync(Menu menu);
    Task<Menu> UpdateAsync(Menu menu);
    Task<bool> DeleteAsync(int id);
}
```

### 4.2. IMenuPermissionService

```csharp
public interface IMenuPermissionService
{
    Task<IReadOnlyList<MenuPermission>> GetRolePermissionsAsync(int roleId);

    Task<bool> SaveRolePermissionAsync(MenuPermission permission);

    Task<bool> RemoveRolePermissionAsync(int roleId, int menuId);

    // Kullanıcı özelinde yetki kontrolü
    Task<bool> HasPermissionAsync(int userId, string menuName, CrudOperation operation);
}

public enum CrudOperation
{
    Create,
    Read,
    Update,
    Delete
}
```

### 4.3. IRoleService (Özet)

```csharp
public interface IRoleService
{
    Task<IReadOnlyList<Role>> GetAllAsync();
    Task<Role> GetByIdAsync(int id);
    Task<Role> CreateAsync(Role role);
    Task<Role> UpdateAsync(Role role);
    Task<bool> DeleteAsync(int id);

    Task<IReadOnlyList<Role>> GetUserRolesAsync(int userId);
    Task<bool> AssignRoleAsync(int userId, int roleId);
    Task<bool> RemoveRoleAsync(int userId, int roleId);
}
```

---

## 5. Menü Ağacı Üretme ve Yetki Mantığı

### 5.1. Kurallar

- `IsAdmin == true` kullanıcı:
  - Tüm aktif menüleri görür (`Menu.IsActive == true`).
  - Tüm menüler için `CanCreate/CanRead/CanUpdate/CanDelete = true` kabul edilir.
- `IsAdmin == false` kullanıcı:
  - Kullanıcının rollerinden gelen `MenuPermission` kayıtları birleştirilir (OR mantığı).
  - En az bir rolde `CanRead == true` ise, kullanıcı o menüyü görebilir.
  - Alt menülerin en az biri görünüyorsa, parent menü gösterilir.

### 5.2. Örnek `MenuService` Uygulaması (Basitleştirilmiş)

```csharp
public class MenuService : IMenuService
{
    private readonly IRepository<Menu> _menuRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<MenuPermission> _permRepo;

    public MenuService(
        IRepository<Menu> menuRepo,
        IRepository<User> userRepo,
        IRepository<MenuPermission> permRepo)
    {
        _menuRepo = menuRepo;
        _userRepo = userRepo;
        _permRepo = permRepo;
    }

    public async Task<UserMenuModel> GetUserMenuAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        var allMenus = (await _menuRepo.GetAllAsync(m => m.IsActive))
            .OrderBy(m => m.Order)
            .ToList();

        if (user.IsAdmin)
        {
            var adminMenus = BuildMenuTree(allMenus, null,
                _ => new PermissionSnapshot(true, true, true, true));

            return new UserMenuModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                IsAdmin = true,
                Menus = adminMenus
            };
        }

        var roleIds = user.Roles.Select(r => r.RoleId).ToList();

        var perms = (await _permRepo
                .GetAllAsync(p => roleIds.Contains(p.RoleId)))
            .GroupBy(p => p.MenuId)
            .ToDictionary(
                g => g.Key,
                g => new PermissionSnapshot(
                    canCreate: g.Any(x => x.CanCreate),
                    canRead: g.Any(x => x.CanRead),
                    canUpdate: g.Any(x => x.CanUpdate),
                    canDelete: g.Any(x => x.CanDelete))
            );

        var userMenus = BuildMenuTree(allMenus, perms, id =>
        {
            return perms.TryGetValue(id, out var p)
                ? p
                : PermissionSnapshot.None;
        });

        return new UserMenuModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            IsAdmin = false,
            Menus = userMenus
        };
    }

    private static List<MenuDto> BuildMenuTree(
        List<Menu> allMenus,
        IDictionary<int, PermissionSnapshot>? rawPerms,
        Func<int, PermissionSnapshot> permSelector)
    {
        var lookup = allMenus.ToLookup(m => m.ParentId);

        List<MenuDto> Build(int? parentId)
        {
            var result = new List<MenuDto>();

            foreach (var menu in lookup[parentId].OrderBy(m => m.Order))
            {
                var perm = permSelector(menu.Id);

                var children = Build(menu.Id);

                // Eğer hiç read yetkisi yok ve alt menü de yoksa, bu menü görünmez.
                var canShow = perm.CanRead || children.Any();
                if (!canShow)
                    continue;

                var dto = new MenuDto
                {
                    Id = menu.Id,
                    Name = menu.Name,
                    Title = menu.Title,
                    Url = menu.Url,
                    IconCss = menu.IconCss,
                    ParentId = menu.ParentId,
                    Order = menu.Order,
                    CanCreate = perm.CanCreate,
                    CanRead = perm.CanRead,
                    CanUpdate = perm.CanUpdate,
                    CanDelete = perm.CanDelete,
                    Children = children
                };

                result.Add(dto);
            }

            return result;
        }

        return Build(null);
    }

    private readonly record struct PermissionSnapshot(
        bool CanCreate,
        bool CanRead,
        bool CanUpdate,
        bool CanDelete)
    {
        public bool CanCreate { get; } = CanCreate;
        public bool CanRead { get; } = CanRead;
        public bool CanUpdate { get; } = CanUpdate;
        public bool CanDelete { get; } = CanDelete;

        public static PermissionSnapshot None =>
            new PermissionSnapshot(false, false, false, false);
    }
}
```

---

## 6. Razor Pages Kullanımı (Tamamen Dinamik)

### 6.1. Dashboard Layout Model (`DashboardLayoutModel`)

```csharp
public class DashboardLayoutModel : PageModel
{
    private readonly IMenuService _menuService;

    public DashboardLayoutModel(IMenuService menuService)
    {
        _menuService = menuService;
    }

    public UserMenuModel UserMenu { get; private set; }

    public async Task OnGetAsync()
    {
        // Burada kendi UserId alma metodunuzu kullanın
        var userId = int.Parse(User.FindFirst("sub").Value);
        UserMenu = await _menuService.GetUserMenuAsync(userId);
    }
}
```

### 6.2. `_DashboardLayout.cshtml` İçinde Sol Menü Render

```razor
@model DashboardLayoutModel

<div class="layout">
    <aside class="sidebar">
        <div class="sidebar-header">
            <span>@Model.UserMenu.UserName</span>
            @if (Model.UserMenu.IsAdmin)
            {
                <span class="badge bg-danger ms-2">Admin</span>
            }
        </div>

        <ul class="nav flex-column">
            @foreach (var menu in Model.UserMenu.Menus)
            {
                @await Html.PartialAsync("_SidebarMenuItem", menu)
            }
        </ul>
    </aside>

    <main class="content">
        @RenderBody()
    </main>
</div>
```

### 6.3. `_SidebarMenuItem.cshtml` (Recursive Partial)

```razor
@model MenuDto

@if (Model.CanRead)
{
    if (Model.Children?.Count > 0)
    {
        <li class="nav-item">
            <button class="nav-link d-flex align-items-center" type="button"
                    data-bs-toggle="collapse" data-bs-target="#m-@Model.Id">
                @if (!string.IsNullOrWhiteSpace(Model.IconCss))
                {
                    <i class="@Model.IconCss me-2"></i>
                }
                <span>@Model.Title</span>
                <i class="bi bi-chevron-down ms-auto small"></i>
            </button>

            <ul class="nav flex-column collapse" id="m-@Model.Id">
                @foreach (var child in Model.Children)
                {
                    @await Html.PartialAsync("_SidebarMenuItem", child)
                }
            </ul>
        </li>
    }
    else
    {
        <li class="nav-item">
            <a class="nav-link d-flex align-items-center" href="@Model.Url">
                @if (!string.IsNullOrWhiteSpace(Model.IconCss))
                {
                    <i class="@Model.IconCss me-2"></i>
                }
                <span>@Model.Title</span>
            </a>
        </li>
    }
}
```

---

## 7. Sayfa Bazlı CRUD Yetki Kullanımı

Her Razor Page kendi menü adı veya Id bilgisine göre yetki çekebilir.

### 7.1. Örnek: `Products` Sayfası Modeli

```csharp
public class ProductsModel : PageModel
{
    private readonly IMenuPermissionService _permissionService;

    private const string MenuName = "Products"; // Menu.Name ile eşleşecek

    public bool CanCreate { get; private set; }
    public bool CanUpdate { get; private set; }
    public bool CanDelete { get; private set; }

    public IReadOnlyList<ProductDto> Products { get; private set; }

    public ProductsModel(IMenuPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = int.Parse(User.FindFirst("sub").Value);

        CanCreate = await _permissionService.HasPermissionAsync(userId, MenuName, CrudOperation.Create);
        CanUpdate = await _permissionService.HasPermissionAsync(userId, MenuName, CrudOperation.Update);
        CanDelete = await _permissionService.HasPermissionAsync(userId, MenuName, CrudOperation.Delete);

        // Products verisini kendi servisinizden doldurun
        // Products = await _productService.GetAllAsync();

        return Page();
    }
}
```

### 7.2. Razor Tarafı

```razor
@page
@model ProductsModel

<h2>Ürünler</h2>

@if (Model.CanCreate)
{
    <a asp-page="./Create" class="btn btn-primary mb-3">
        <i class="bi bi-plus-circle"></i> Yeni Ürün
    </a>
}

<table class="table table-striped">
    <thead>
    <tr>
        <th>Ad</th>
        <th>Fiyat</th>
        @if (Model.CanUpdate || Model.CanDelete)
        {
            <th style="width: 140px;">İşlemler</th>
        }
    </tr>
    </thead>
    <tbody>
    @foreach (var item in Model.Products)
    {
        <tr>
            <td>@item.Name</td>
            <td>@item.Price.ToString("N2")</td>
            @if (Model.CanUpdate || Model.CanDelete)
            {
                <td>
                    @if (Model.CanUpdate)
                    {
                        <a asp-page="./Edit" asp-route-id="@item.Id" class="btn btn-sm btn-outline-warning me-1">
                            <i class="bi bi-pencil"></i>
                        </a>
                    }

                    @if (Model.CanDelete)
                    {
                        <button type="button" class="btn btn-sm btn-outline-danger">
                            <i class="bi bi-trash"></i>
                        </button>
                    }
                </td>
            }
        </tr>
    }
    </tbody>
</table>
```

---

## 8. Özet

- Menü yapısı tamamen **class tabanlı ve hiyerarşik** tasarlandı.
- Yetkilendirme, **User → Role → MenuPermission** üzerinden çalışıyor.
- `IsAdmin` kullanıcılar için tüm kısıtlamalar bypass ediliyor.
- Razor Pages tarafında sidebar ve sayfa aksiyonları **daima `foreach` ve permission kontrolleriyle** dinamik olarak oluşuyor.

Bu dokümanı `WebHost.Prod` tasarımında temel mimari rehber olarak kullanabilirsiniz. İhtiyaca göre entity ve DTO alanlarını genişletmek kolaydır.