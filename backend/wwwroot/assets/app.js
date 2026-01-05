const API_BASE = window.location.origin.startsWith("http")
    ? window.location.origin
    : (localStorage.getItem("apiBase") || "https://localhost:7070");

function getToken() {
    return localStorage.getItem("token");
}

function setToken(token) {
    localStorage.setItem("token", token);
}

function clearAuth() {
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userType");
}

function setNotice(target, message, isSuccess = false) {
    if (!target) return;
    target.textContent = message;
    target.classList.toggle("success", isSuccess);
    target.style.display = message ? "block" : "none";
}

function updateNav() {
    const navLinks = document.querySelectorAll("[data-nav]");
    if (navLinks.length === 0) return;

    const token = getToken();
    const userType = Number(localStorage.getItem("userType") || 0);
    const isLoginPage = window.location.pathname.endsWith("/index.html") || window.location.pathname.endsWith("/");

    navLinks.forEach((link) => {
        link.style.display = "none";
    });

    if (isLoginPage) {
        const loginLink = document.querySelector('[data-nav="login"]');
        if (loginLink) {
            loginLink.style.display = "inline";
        }
        return;
    }

    if (!token) {
        const loginLink = document.querySelector('[data-nav="login"]');
        if (loginLink) {
            loginLink.style.display = "inline";
        }
        return;
    }

    if (userType === 0) {
        document.querySelector('[data-nav="menu"]')?.style.setProperty("display", "inline");
        document.querySelector('[data-nav="orders"]')?.style.setProperty("display", "inline");
    } else if (userType === 1) {
        document.querySelector('[data-nav="merchant"]')?.style.setProperty("display", "inline");
    } else if (userType === 2) {
        document.querySelector('[data-nav="admin"]')?.style.setProperty("display", "inline");
    }
}

async function apiFetch(path, options = {}) {
    const headers = {
        "Content-Type": "application/json",
        ...(options.headers || {})
    };
    const token = getToken();
    if (token) {
        headers.Authorization = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE}${path}`, {
        ...options,
        headers
    });

    if (!response.ok) {
        const payload = await response.json().catch(() => ({}));
        const message = payload.message || "请求失败";
        throw new Error(message);
    }

    return response.json();
}

function requireAuth() {
    if (!getToken()) {
        window.location.href = "index.html";
        return false;
    }
    return true;
}

function requireAdmin() {
    if (!requireAuth()) return false;
    const userType = Number(localStorage.getItem("userType") || 0);
    if (userType !== 2) {
        window.location.href = "menu.html";
        return false;
    }
    return true;
}

function requireMerchant() {
    if (!requireAuth()) return false;
    const userType = Number(localStorage.getItem("userType") || 0);
    if (userType !== 1) {
        window.location.href = "menu.html";
        return false;
    }
    return true;
}

function bindLogin() {
    const form = document.querySelector("[data-login-form]");
    if (!form) return;

    const notice = document.querySelector("[data-login-notice]");
    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        setNotice(notice, "");

        const userName = document.querySelector("#username")?.value.trim();
        const password = document.querySelector("#password")?.value.trim();
        const role = document.querySelector("#role")?.value;

        if (!userName || !password) {
            setNotice(notice, "请输入用户名和密码");
            return;
        }

        try {
            const response = await apiFetch("/api/auth/login", {
                method: "POST",
                body: JSON.stringify({ userName, password })
            });

            const roleMap = { 用户: 0, 商家: 1, 管理员: 2 };
            const selectedType = roleMap[role] ?? 0;
            if (response.userType !== selectedType) {
                setNotice(notice, "身份不匹配，请选择正确的角色");
                return;
            }

            setToken(response.token);
            localStorage.setItem("userName", response.userName);
            localStorage.setItem("userType", response.userType);

            if (response.userType === 2) {
                window.location.href = "admin.html";
            } else if (response.userType === 1) {
                window.location.href = "merchant.html";
            } else {
                window.location.href = "menu.html";
            }
        } catch (error) {
            setNotice(notice, error.message || "登录失败");
        }
    });
}

async function renderMerchants() {
    const container = document.querySelector("[data-merchant-list]");
    if (!container) return;

    container.innerHTML = "";
    const merchants = await apiFetch("/api/merchants");
    merchants.forEach((merchant) => {
        const item = document.createElement("div");
        item.className = "list-item fade-up";
        item.innerHTML = `
            <div class="pill">商家</div>
            <div>
                <h4>${merchant.shopName}</h4>
                <p>${merchant.shopAddress}</p>
            </div>
            <button class="btn secondary" data-merchant-id="${merchant.merchantId}">查看菜单</button>
        `;
        container.appendChild(item);
    });

    return merchants;
}

async function renderProducts(merchantId) {
    const container = document.querySelector("[data-product-list]");
    if (!container) return;

    container.innerHTML = "";
    const products = await apiFetch(`/api/products?merchantId=${merchantId}`);
    const filtered = products.filter((product) => product.productName !== "招牌红烧肉");
    filtered.forEach((product) => {
        const item = document.createElement("div");
        item.className = "list-item fade-up";
        
        let imgSrc = product.imageUrl;
        if (imgSrc && imgSrc.startsWith('/')) {
            imgSrc = `${API_BASE.replace('/api', '')}${imgSrc}`;
        }
        if (!imgSrc) {
            imgSrc = 'https://via.placeholder.com/80?text=No+Img';
        }

        item.innerHTML = `
            <div style="width: 80px; height: 80px; border-radius: 8px; overflow: hidden; flex-shrink: 0; margin-right: 12px;">
                <img src="${imgSrc}" style="width: 100%; height: 100%; object-fit: cover;" alt="${product.productName}">
            </div>
            <div style="flex: 1;">
                <div style="display: flex; justify-content: space-between; align-items: flex-start;">
                    <h4 style="margin: 0;">${product.productName}</h4>
                    <div class="badge">￥${Number(product.price).toFixed(2)}</div>
                </div>
                <p style="margin: 4px 0 8px; font-size: 12px; color: var(--muted);">${product.description || "暂无描述"}</p>
                <button class="btn sm" data-add-product="${product.productId}">加入购物车</button>
            </div>
        `;
        // 修改样式以支持横向布局
        item.style.display = "flex";
        item.style.alignItems = "center";
        
        container.appendChild(item);
    });
}

function bindMenuPage() {
    const merchantList = document.querySelector("[data-merchant-list]");
    if (!merchantList) return;
    if (!requireAuth()) return;

    const notice = document.querySelector("[data-menu-notice]");
    const cartContainer = document.querySelector("[data-cart-items]");
    const totalEl = document.querySelector("[data-cart-total]");
    const submitBtn = document.querySelector("[data-order-submit]");
    const addressInput = document.querySelector("[data-delivery-address]");
    const merchantLabel = document.querySelector("[data-current-merchant]");
    const clearCartBtn = document.querySelector("[data-clear-cart]");
    const logoutBtn = document.querySelector("[data-logout]");
    const paymentModal = document.querySelector("[data-payment-modal]");
    const modalCloseBtn = document.querySelector("[data-modal-close]");
    const modalPayBtn = document.querySelector("[data-modal-pay]");
    const modalAmount = document.querySelector("[data-modal-amount]");

    let cart = [];
    let currentMerchantId = null;
    let productCache = new Map();
    let merchantMap = new Map();
    let lastOrderId = null;

    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            clearAuth();
            window.location.href = "index.html";
        });
    }

    renderMerchants()
        .then((merchants) => {
            merchantMap = new Map(merchants.map((m) => [m.merchantId, m.shopName]));
            merchantList.addEventListener("click", async (event) => {
                const target = event.target;
                if (!(target instanceof HTMLElement)) return;
                const merchantId = target.dataset.merchantId;
                if (!merchantId) return;
                currentMerchantId = Number(merchantId);
                if (merchantLabel) {
                    merchantLabel.textContent = merchantMap.get(currentMerchantId) || "已选商家";
                }
                cart = [];
                renderCart();
                try {
                    await renderProducts(currentMerchantId);
                } catch (error) {
                    setNotice(notice, error.message);
                }
            });
        })
        .catch((error) => setNotice(notice, error.message));

    if (clearCartBtn) {
        clearCartBtn.addEventListener("click", () => {
            cart = [];
            renderCart();
            setNotice(notice, "购物车已清空", true);
        });
    }

    if (modalCloseBtn) {
        modalCloseBtn.addEventListener("click", () => {
            if (paymentModal) paymentModal.style.display = "none";
        });
    }

    if (modalPayBtn) {
        modalPayBtn.addEventListener("click", async () => {
            if (!lastOrderId) {
                if (paymentModal) paymentModal.style.display = "none";
                return;
            }
            try {
                await apiFetch(`/api/payments/${lastOrderId}/pay`, {
                    method: "POST",
                    body: JSON.stringify({ paymentMethod: "在线支付" })
                });
                setNotice(notice, "支付成功", true);
                if (paymentModal) paymentModal.style.display = "none";
            } catch (error) {
                setNotice(notice, error.message);
            }
        });
    }

    document.body.addEventListener("click", async (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) return;
        const productId = target.dataset.addProduct;
        const incId = target.dataset.cartInc;
        const decId = target.dataset.cartDec;

        // 新增菜品到购物车
        if (productId) {
            try {
                let product = productCache.get(Number(productId));
                if (!product) {
                    product = await apiFetch(`/api/products?merchantId=${currentMerchantId || 0}`)
                        .then((items) => items.find((item) => item.productId === Number(productId)));
                    if (product) {
                        productCache.set(product.productId, product);
                    }
                }
                if (!product) return;

                const existing = cart.find((item) => item.productId === product.productId);
                if (existing) {
                    existing.quantity += 1;
                } else {
                    cart.push({
                        productId: product.productId,
                        name: product.productName,
                        price: Number(product.price),
                        quantity: 1
                    });
                }
                renderCart();
            } catch (error) {
                setNotice(notice, error.message);
            }
            return;
        }

        // 购物车内 +/-
        if (incId || decId) {
            const id = Number(incId || decId);
            const item = cart.find((c) => c.productId === id);
            if (!item) return;

            if (incId) {
                item.quantity += 1;
            } else if (decId) {
                item.quantity -= 1;
                if (item.quantity <= 0) {
                    cart = cart.filter((c) => c.productId !== id);
                }
            }
            renderCart();
        }

    });

    function renderCart() {
        if (!cartContainer || !totalEl) return;
        if (cart.length === 0) {
            cartContainer.innerHTML = `<p class="muted" style="margin: 0;">暂未选择菜品</p>`;
            totalEl.textContent = "￥0.00";
            return;
        }

        cartContainer.innerHTML = cart
            .map(
                (item) => `
                <div class="cart-line">
                    <div>
                        <div class="cart-line-name">${item.name}</div>
                        <div class="cart-line-meta">￥${item.price.toFixed(2)} / 份</div>
                    </div>
                    <div class="cart-line-qty">
                        <button class="cart-qty-btn" data-cart-dec="${item.productId}">-</button>
                        <span>x${item.quantity}</span>
                        <button class="cart-qty-btn" data-cart-inc="${item.productId}">+</button>
                    </div>
                    <div class="cart-line-price">￥${(item.price * item.quantity).toFixed(2)}</div>
                </div>
            `
            )
            .join("");

        const total = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
        totalEl.textContent = `￥${total.toFixed(2)}`;
    }

    if (submitBtn) {
        submitBtn.addEventListener("click", async () => {
            setNotice(notice, "");
            if (!currentMerchantId) {
                setNotice(notice, "请先选择商家");
                return;
            }
            if (cart.length === 0) {
                setNotice(notice, "购物车为空");
                return;
            }
            const address = addressInput?.value.trim();
            if (!address) {
                setNotice(notice, "请输入配送地址");
                return;
            }

            try {
                const totalAmount = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
                await apiFetch("/api/orders", {
                    method: "POST",
                    body: JSON.stringify({
                        merchantId: currentMerchantId,
                        deliveryAddress: address,
                        items: cart.map((item) => ({
                            productId: item.productId,
                            quantity: item.quantity
                        }))
                    })
                }).then((res) => {
                    lastOrderId = res?.orderId;
                });
                cart = [];
                renderCart();
                setNotice(notice, "订单创建成功", true);
                if (modalAmount) modalAmount.textContent = `￥${totalAmount.toFixed(2)}`;
                if (paymentModal) paymentModal.style.display = "flex";
            } catch (error) {
                setNotice(notice, error.message);
            }
        });
    }
}

async function renderOrders() {
    const table = document.querySelector("[data-orders-table]");
    if (!table) return;
    if (!requireAuth()) return;

    try {
        const notice = document.querySelector("[data-orders-notice]");
        const merchants = await apiFetch("/api/merchants");
        const merchantMap = new Map(
            merchants.map((merchant) => [merchant.merchantId, merchant.shopName])
        );
        const orders = await apiFetch("/api/orders/my");
        setNotice(notice, "", false);
        table.innerHTML = orders
            .map((order) => {
                const statusMap = {
                    0: { label: "待接单", cls: "pending" },
                    1: { label: "已接单", cls: "delivering" },
                    2: { label: "配送中", cls: "delivering" },
                    3: { label: "已完成", cls: "completed" },
                    4: { label: "已取消", cls: "pending" }
                };
                const status = statusMap[order.orderStatus] || { label: "未知", cls: "pending" };
                const payStatusMap = {
                    0: { label: "待支付", cls: "pending" },
                    1: { label: "已支付", cls: "completed" },
                    2: { label: "支付失败", cls: "pending" }
                };
                const payStatus = payStatusMap[order.payStatus] || { label: "未知", cls: "pending" };
                const payAction = order.payStatus === 0
                    ? `<button class="btn secondary" data-pay-order="${order.orderId}">去支付</button>`
                    : `<span class="status ${payStatus.cls}">${payStatus.label}</span>`;

                return `
                    <tr>
                        <td>#${order.orderNumber}</td>
                        <td>${merchantMap.get(order.merchantId) || order.merchantId}</td>
                        <td>${new Date(order.orderTime).toLocaleString()}</td>
                        <td>￥${Number(order.orderAmount).toFixed(2)}</td>
                        <td><span class="status ${status.cls}">${status.label}</span></td>
                        <td>${payAction}</td>
                    </tr>
                `;
            })
            .join("");
    } catch (error) {
        const notice = document.querySelector("[data-orders-notice]");
        setNotice(notice, error.message || "加载订单失败");
        table.innerHTML = `<tr><td colspan="5">暂无订单</td></tr>`;
    }
}

function bindOrderPayments() {
    const table = document.querySelector("[data-orders-table]");
    if (!table) return;

    document.body.addEventListener("click", async (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) return;
        const payId = target.dataset.payOrder;
        if (!payId) return;
        const notice = document.querySelector("[data-orders-notice]");
        try {
            await apiFetch(`/api/payments/${payId}/pay`, {
                method: "POST",
                body: JSON.stringify({ paymentMethod: "在线支付" })
            });
            setNotice(notice, "支付成功", true);
            renderOrders();
        } catch (error) {
            setNotice(notice, error.message || "支付失败");
        }
    });
}

async function loadSummary() {
    const totalOrders = document.querySelector("[data-total-orders]");
    const totalRevenue = document.querySelector("[data-total-revenue]");
    const todayOrders = document.querySelector("[data-today-orders]");
    if (!totalOrders || !totalRevenue || !todayOrders) return;

    try {
        const summary = await apiFetch("/api/admin/statistics");
        totalOrders.textContent = summary.totalOrders ?? 0;
        totalRevenue.textContent = `￥${Number(summary.totalRevenue ?? 0).toFixed(2)}`;
        todayOrders.textContent = summary.todayOrders ?? 0;
    } catch {
        totalOrders.textContent = "-";
        totalRevenue.textContent = "-";
        todayOrders.textContent = "-";
    }
}

async function loadMerchants(selectEl) {
    const merchants = await apiFetch("/api/merchants");
    selectEl.innerHTML = merchants
        .map((merchant) => `<option value="${merchant.merchantId}">${merchant.shopName}</option>`)
        .join("");
    return merchants;
}

async function loadProducts(merchantId) {
    return apiFetch(`/api/admin/products?merchantId=${merchantId}`);
}

async function renderProductsTable(merchantId) {
    const tableBody = document.querySelector("[data-products-body]");
    if (!tableBody) return;

    const products = await loadProducts(merchantId);
    tableBody.innerHTML = products
        .map(
            (product) => `
            <tr data-product-row="${product.productId}">
                <td>${product.productId}</td>
                <td>${product.productName}</td>
                <td>￥${Number(product.price).toFixed(2)}</td>
                <td>${product.isAvailable ? "上架" : "下架"}</td>
                <td>
                    <button class="btn secondary" data-edit-product="${product.productId}">编辑</button>
                </td>
            </tr>
        `
        )
        .join("");

    return products;
}

async function renderOrdersTable() {
    const tableBody = document.querySelector("[data-orders-body]");
    if (!tableBody) return;

    const orders = await apiFetch("/api/admin/orders");
    tableBody.innerHTML = orders
        .map((order) => {
            const statusMap = {
                0: { label: "待接单", cls: "pending" },
                1: { label: "已接单", cls: "delivering" },
                2: { label: "配送中", cls: "delivering" },
                3: { label: "已完成", cls: "completed" },
                4: { label: "已取消", cls: "pending" }
            };
            const status = statusMap[order.orderStatus] || { label: "未知", cls: "pending" };
            return `
                <tr>
                    <td>${order.orderNumber}</td>
                    <td>${order.userId}</td>
                    <td>${order.merchantId}</td>
                    <td>￥${Number(order.orderAmount).toFixed(2)}</td>
                    <td><span class="status ${status.cls}">${status.label}</span></td>
                    <td>
                        <select data-status-select="${order.orderId}">
                            <option value="0" ${order.orderStatus === 0 ? "selected" : ""}>待接单</option>
                            <option value="1" ${order.orderStatus === 1 ? "selected" : ""}>已接单</option>
                            <option value="2" ${order.orderStatus === 2 ? "selected" : ""}>配送中</option>
                            <option value="3" ${order.orderStatus === 3 ? "selected" : ""}>已完成</option>
                            <option value="4" ${order.orderStatus === 4 ? "selected" : ""}>已取消</option>
                        </select>
                    </td>
                    <td>
                        <button class="btn secondary" data-update-order="${order.orderId}">更新</button>
                    </td>
                </tr>
            `;
        })
        .join("");
}

function bindAdminDashboard() {
    const page = document.querySelector("[data-admin-dashboard]");
    if (!page) return;
    if (!requireAdmin()) return;

    const notice = document.querySelector("[data-admin-notice]");
    const merchantSelect = document.querySelector("[data-merchant-select]");
    const form = document.querySelector("[data-product-form]");
    const logoutBtn = document.querySelector("[data-logout]");
    const refreshOrdersBtn = document.querySelector("[data-refresh-orders]");

    let currentMerchantId = null;
    let cachedProducts = [];
    let editingProductId = null;

    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            clearAuth();
            window.location.href = "index.html";
        });
    }

    loadSummary();

    loadMerchants(merchantSelect)
        .then(() => {
            currentMerchantId = Number(merchantSelect.value);
            return renderProductsTable(currentMerchantId);
        })
        .then((products) => {
            cachedProducts = products || [];
        })
        .catch((error) => setNotice(notice, error.message));

    merchantSelect?.addEventListener("change", async () => {
        setNotice(notice, "");
        currentMerchantId = Number(merchantSelect.value);
        try {
            cachedProducts = await renderProductsTable(currentMerchantId);
        } catch (error) {
            setNotice(notice, error.message);
        }
    });

    document.body.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) return;

        const editId = target.dataset.editProduct;
        if (editId) {
            const product = cachedProducts.find((item) => item.productId === Number(editId));
            if (!product || !form) return;
            editingProductId = product.productId;
            form.querySelector("[data-product-name]").value = product.productName;
            form.querySelector("[data-product-price]").value = product.price;
            form.querySelector("[data-product-desc]").value = product.description || "";
            form.querySelector("[data-product-available]").checked = product.isAvailable;
            return;
        }

        const updateOrderId = target.dataset.updateOrder;
        if (updateOrderId) {
            const select = document.querySelector(`[data-status-select="${updateOrderId}"]`);
            const status = Number(select?.value ?? 0);
            apiFetch(`/api/admin/orders/${updateOrderId}/status`, {
                method: "PUT",
                body: JSON.stringify({ orderStatus: status })
            })
                .then(() => {
                    setNotice(notice, "订单状态已更新", true);
                    renderOrdersTable();
                })
                .catch((error) => setNotice(notice, error.message));
        }
    });

    form?.addEventListener("submit", async (event) => {
        event.preventDefault();
        setNotice(notice, "");

        const productName = form.querySelector("[data-product-name]").value.trim();
        const price = Number(form.querySelector("[data-product-price]").value);
        const description = form.querySelector("[data-product-desc]").value.trim();
        const isAvailable = form.querySelector("[data-product-available]").checked;

        if (!productName || !price || price <= 0) {
            setNotice(notice, "请输入正确的菜品名称和价格");
            return;
        }

        try {
            if (editingProductId) {
                await apiFetch(`/api/admin/products/${editingProductId}`, {
                    method: "PUT",
                    body: JSON.stringify({
                        merchantId: currentMerchantId,
                        productName,
                        price,
                        description,
                        isAvailable
                    })
                });
                setNotice(notice, "菜品已更新", true);
            } else {
                await apiFetch("/api/admin/products", {
                    method: "POST",
                    body: JSON.stringify({
                        merchantId: currentMerchantId,
                        productName,
                        price,
                        description,
                        isAvailable
                    })
                });
                setNotice(notice, "菜品已新增", true);
            }

            editingProductId = null;
            form.reset();
            cachedProducts = await renderProductsTable(currentMerchantId);
        } catch (error) {
            setNotice(notice, error.message);
        }
    });

    if (refreshOrdersBtn) {
        refreshOrdersBtn.addEventListener("click", () => {
            renderOrdersTable().catch((error) => setNotice(notice, error.message));
        });
    }

    renderOrdersTable().catch((error) => setNotice(notice, error.message));
}

async function renderMerchantProducts() {
    const tableBody = document.querySelector("[data-merchant-products-body]");
    if (!tableBody) return;

    const products = await apiFetch("/api/merchant/products");
    tableBody.innerHTML = products
        .map(
            (product) => {
                let imgSrc = product.imageUrl;
                if (imgSrc && imgSrc.startsWith('/')) {
                    imgSrc = `${API_BASE.replace('/api', '')}${imgSrc}`;
                }
                const imgHtml = imgSrc 
                    ? `<img src="${imgSrc}" style="width: 40px; height: 40px; object-fit: cover; border-radius: 4px;">` 
                    : `<span style="color: #ccc;">无图</span>`;

                return `
                <tr data-product-row="${product.productId}">
                    <td>${product.productId}</td>
                    <td>
                        <div style="display: flex; align-items: center; gap: 8px;">
                            ${imgHtml}
                            <span>${product.productName}</span>
                        </div>
                    </td>
                    <td>￥${Number(product.price).toFixed(2)}</td>
                    <td>${product.isAvailable ? "上架" : "下架"}</td>
                    <td>
                        <button class="btn secondary" data-merchant-edit="${product.productId}">编辑</button>
                    </td>
                </tr>
            `;
            }
        )
        .join("");

    return products;
}

async function renderMerchantOrders() {
    const tableBody = document.querySelector("[data-merchant-orders-body]");
    if (!tableBody) return;

    const orders = await apiFetch("/api/merchant/orders");
    tableBody.innerHTML = orders
        .map((order) => {
            const statusMap = {
                0: { label: "待接单", cls: "pending" },
                1: { label: "已接单", cls: "delivering" },
                2: { label: "配送中", cls: "delivering" },
                3: { label: "已完成", cls: "completed" },
                4: { label: "已取消", cls: "pending" }
            };
            const status = statusMap[order.orderStatus] || { label: "未知", cls: "pending" };
            return `
                <tr>
                    <td>${order.orderNumber}</td>
                    <td>${order.userId}</td>
                    <td>￥${Number(order.orderAmount).toFixed(2)}</td>
                    <td><span class="status ${status.cls}">${status.label}</span></td>
                    <td>
                        <select data-merchant-status="${order.orderId}">
                            <option value="0" ${order.orderStatus === 0 ? "selected" : ""}>待接单</option>
                            <option value="1" ${order.orderStatus === 1 ? "selected" : ""}>已接单</option>
                            <option value="2" ${order.orderStatus === 2 ? "selected" : ""}>配送中</option>
                            <option value="3" ${order.orderStatus === 3 ? "selected" : ""}>已完成</option>
                            <option value="4" ${order.orderStatus === 4 ? "selected" : ""}>已取消</option>
                        </select>
                    </td>
                    <td>
                        <button class="btn secondary" data-merchant-update="${order.orderId}">更新</button>
                    </td>
                </tr>
            `;
        })
        .join("");
}

function bindMerchantDashboard() {
    const page = document.querySelector("[data-merchant-dashboard]");
    if (!page) return;
    if (!requireMerchant()) return;

    const notice = document.querySelector("[data-merchant-notice]");
    const form = document.querySelector("[data-merchant-product-form]");
    const logoutBtn = document.querySelector("[data-logout]");
    const refreshOrdersBtn = document.querySelector("[data-merchant-refresh-orders]");

    let cachedProducts = [];
    let editingProductId = null;

    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            clearAuth();
            window.location.href = "index.html";
        });
    }

    renderMerchantProducts()
        .then((products) => {
            cachedProducts = products || [];
        })
        .catch((error) => setNotice(notice, error.message));

    renderMerchantOrders().catch((error) => setNotice(notice, error.message));

    document.body.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) return;

        const editId = target.dataset.merchantEdit;
        if (editId) {
            const product = cachedProducts.find((item) => item.productId === Number(editId));
            if (!product || !form) return;
            editingProductId = product.productId;
            form.querySelector("[data-product-name]").value = product.productName;
            form.querySelector("[data-product-price]").value = product.price;
            form.querySelector("[data-product-desc]").value = product.description || "";
            form.querySelector("[data-product-image]").value = product.imageUrl || "";
            form.querySelector("[data-product-available]").checked = product.isAvailable;
            return;
        }

        const updateOrderId = target.dataset.merchantUpdate;
        if (updateOrderId) {
            const select = document.querySelector(`[data-merchant-status="${updateOrderId}"]`);
            const status = Number(select?.value ?? 0);
            apiFetch(`/api/merchant/orders/${updateOrderId}/status`, {
                method: "PUT",
                body: JSON.stringify({ orderStatus: status })
            })
                .then(() => {
                    setNotice(notice, "订单状态已更新", true);
                    renderMerchantOrders();
                })
                .catch((error) => setNotice(notice, error.message));
        }
    });

    form?.addEventListener("submit", async (event) => {
        event.preventDefault();
        setNotice(notice, "");

        const productName = form.querySelector("[data-product-name]").value.trim();
        const price = Number(form.querySelector("[data-product-price]").value);
        const description = form.querySelector("[data-product-desc]").value.trim();
        const imageUrl = form.querySelector("[data-product-image]").value.trim();
        const isAvailable = form.querySelector("[data-product-available]").checked;

        if (!productName || !price || price <= 0) {
            setNotice(notice, "请输入正确的菜品名称和价格");
            return;
        }

        try {
            if (editingProductId) {
                await apiFetch(`/api/merchant/products/${editingProductId}`, {
                    method: "PUT",
                    body: JSON.stringify({
                        productName,
                        price,
                        description,
                        imageUrl,
                        isAvailable
                    })
                });
                setNotice(notice, "菜品已更新", true);
            } else {
                await apiFetch("/api/merchant/products", {
                    method: "POST",
                    body: JSON.stringify({
                        productName,
                        price,
                        description,
                        imageUrl,
                        isAvailable
                    })
                });
                setNotice(notice, "菜品已新增", true);
            }

            editingProductId = null;
            form.reset();
            cachedProducts = await renderMerchantProducts();
        } catch (error) {
            setNotice(notice, error.message);
        }
    });

    if (refreshOrdersBtn) {
        refreshOrdersBtn.addEventListener("click", () => {
            renderMerchantOrders().catch((error) => setNotice(notice, error.message));
        });
    }
}

document.addEventListener("DOMContentLoaded", () => {
    updateNav();
    bindLogin();
    bindMenuPage();
    renderOrders();
    bindOrderPayments();
    bindAdminDashboard();
    bindMerchantDashboard();

    const refreshBtn = document.querySelector("[data-refresh-orders]");
    if (refreshBtn) {
        refreshBtn.addEventListener("click", () => {
            renderOrders();
        });
    }

    const logoutBtn = document.querySelector("[data-logout]");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            clearAuth();
            window.location.href = "index.html";
        });
    }
});
