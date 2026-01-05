# 外卖点餐系统 (WaiMai System)

这是一个基于 **ASP.NET Core 8.0** 和 **原生 HTML/JS/CSS** 开发的外卖点餐系统。
项目包含用户端（点餐、订单）、商家端（菜品管理、订单处理）和管理员端（用户/商家管理、数据监控）。

## 📋 快速开始指南

如果你是项目组成员，请按照以下步骤配置开发环境并运行项目。

### 1. 环境准备

在开始之前，请确保你的电脑上安装了以下软件：

*   **Git**: 用于拉取代码。
*   **.NET 8.0 SDK**: [下载地址](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
*   **SQL Server**: 可以是 SQL Server 2019/2022，或者轻量级的 SQL Server Express / LocalDB。
    *   *推荐安装 SSMS (SQL Server Management Studio) 用于管理数据库。*

### 2. 获取代码

打开终端（CMD 或 PowerShell），执行以下命令克隆项目：

```powershell
git clone https://github.com/hhu79058-bit/waimai_system.git
cd waimai_system
```

### 3. 初始化数据库 (重要!)

这是最关键的一步，必须执行 SQL 脚本来创建数据库和表结构。

1.  打开 SSMS (SQL Server Management Studio) 并连接到你的数据库服务器 (例如 `.` 或 `(localdb)\MSSQLLocalDB`)。
2.  打开文件 `Database/Full_Install.sql`。
3.  点击 **执行 (Execute)**。
    *   *脚本会自动创建名为 `WaiMaiSystem` 的数据库，并建立所有表结构和默认数据。*

**默认账号：**
*   **管理员**: `admin` / `123456`
*   **商家**: `merchant` / `123456`
*   **用户**: `user` / `123456`

### 4. 配置数据库连接

打开项目中的 `backend/appsettings.json` 文件，检查连接字符串：

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=.;Initial Catalog=WaiMaiSystem;Integrated Security=True;TrustServerCertificate=True"
}
```

*   如果你的 SQL Server 实例名不是 `.` (默认实例)，请修改 `Data Source`。
    *   例如使用 LocalDB: `Data Source=(localdb)\MSSQLLocalDB`
    *   例如使用 SQLExpress: `Data Source=.\SQLExpress`

### 5. 运行项目

在项目根目录下，打开终端执行：

```powershell
cd backend
dotnet run
```

当看到类似以下的输出时，说明启动成功：

```
Now listening on: http://localhost:5000
Now listening on: https://localhost:7070
```

### 6. 访问系统

打开浏览器访问控制台输出的 HTTPS 地址（通常是 `https://localhost:7070`）：

*   **首页/登录**: `https://localhost:7070/index.html`
*   **系统会自动跳转到登录页，请选择身份并登录。**

---

## 🛠️ 项目结构

*   `backend/`: 后端 API 项目代码
    *   `Controllers/`: API 控制器
    *   `Services/`: 业务逻辑层
    *   `Models/`: 数据实体
    *   `Data/`: 数据库访问层 (DbHelper)
    *   `wwwroot/`: 前端静态资源 (HTML/CSS/JS)
*   `Database/`: 数据库脚本
    *   `Full_Install.sql`: **一键安装脚本**

## ⚠️ 常见问题

1.  **登录提示“请求失败”**：
    *   检查后端黑框（终端）是否正在运行，不要关闭它。
    *   检查 `appsettings.json` 里的数据库连接字符串是否正确。
2.  **图片无法显示**：
    *   确保 `backend/wwwroot/uploads` 文件夹存在。

---

