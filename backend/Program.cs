using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WaiMai.Backend.Data;
using WaiMai.Backend.Options;
using WaiMai.Backend.Services;

// 创建Web应用构建器
var builder = WebApplication.CreateBuilder(args);

// ===== 注册服务到依赖注入容器 =====

// 添加控制器支持
builder.Services.AddControllers();

// 添加API文档生成（Swagger）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册JWT配置选项（从appsettings.json的"Jwt"节读取）
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// 注册数据访问层服务
builder.Services.AddSingleton<DbHelper>();  // 单例：全局共享一个实例

// 注册业务逻辑层服务（作用域：每个HTTP请求一个实例）
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MerchantService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AddressService>();
builder.Services.AddScoped<OrderLogService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<FileStorageService>();

// 配置JWT身份验证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 读取JWT配置
        JwtOptions jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
        
        // 安全检查：生产环境必须配置强密钥
        if (!builder.Environment.IsDevelopment() && jwt.Key.StartsWith("Replace-", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("JWT Key 未配置，请通过环境变量或配置文件设置强密钥。");
        }
        
        // 配置令牌验证参数
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,                // 验证颁发者
            ValidateAudience = true,              // 验证接收者
            ValidateLifetime = true,              // 验证过期时间
            ValidateIssuerSigningKey = true,      // 验证签名密钥
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });

// 添加授权支持（基于角色）
builder.Services.AddAuthorization();

// 配置CORS（跨域资源共享）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()    // 允许所有来源（生产环境建议限制具体域名）
            .AllowAnyHeader()      // 允许所有请求头
            .AllowAnyMethod();     // 允许所有HTTP方法
    });
});

// ===== 构建应用并配置HTTP请求管道 =====
var app = builder.Build();

// 开发环境启用Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用HTTPS重定向
app.UseHttpsRedirection();

// 使用默认文件和静态文件（如index.html、CSS、JS等）
app.UseDefaultFiles();
app.UseStaticFiles();

// 使用CORS策略
app.UseCors("AllowFrontend");

// 使用身份验证中间件（必须在授权之前）
app.UseAuthentication();

// 使用授权中间件
app.UseAuthorization();

// 映射控制器路由
app.MapControllers();

// 启动应用
app.Run();
