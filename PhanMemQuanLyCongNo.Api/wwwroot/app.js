const tenantId = "11111111-1111-1111-1111-111111111111";
const money = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND", maximumFractionDigits: 0 });ứ
const dateFmt = new Intl.DateTimeFormat("vi-VN");

const els = {
    totalReceivable: document.querySelector("#totalReceivable"),
    overdueAmount: document.querySelector("#overdueAmount"),
    overdueCount: document.querySelector("#overdueCount"),
    totalPaid: document.querySelector("#totalPaid"),
    collectionRate: document.querySelector("#collectionRate"),
    dueSoonCount: document.querySelector("#dueSoonCount"),
    statusBars: document.querySelector("#statusBars"),
    riskList: document.querySelector("#riskList"),
    alertList: document.querySelector("#alertList"),
    notificationList: document.querySelector("#notificationList"),
    debtRows: document.querySelector("#debtRows"),
    searchInput: document.querySelector("#searchInput"),
    statusFilter: document.querySelector("#statusFilter"),
    refreshBtn: document.querySelector("#refreshBtn"),
    debtDialog: document.querySelector("#debtDialog"),
    openCreateDebt: document.querySelector("#openCreateDebt"),
    contractSelect: document.querySelector("#contractSelect"),
    createDebtBtn: document.querySelector("#createDebtBtn"),
    issuedDateInput: document.querySelector("#issuedDateInput"),
    dueDateInput: document.querySelector("#dueDateInput")
};

function api(path, options = {}) {
    return fetch(path, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            "X-Tenant-Id": tenantId,
            "Authorization": "Bearer " + localStorage.getItem("token"),
            ...(options.headers || {})
        }
    }).then(async response => {
        if (!response.ok) {
            const error = await response.json().catch(() => ({ error: "Yêu cầu không thành công" }));
            throw new Error(error.error || "Yêu cầu không thành công");
        }
        return response.json();
    });
}

function toDateInput(date) {
    return date.toISOString().slice(0, 10);
}

function renderDashboard(data) {
    els.totalReceivable.textContent = money.format(data.kpis.totalReceivable);
    els.overdueAmount.textContent = money.format(data.kpis.overdueAmount);
    els.overdueCount.textContent = `${data.kpis.overdueCount} khoản`;
    els.totalPaid.textContent = money.format(data.kpis.totalPaid);
    els.collectionRate.textContent = `Tỷ lệ thu hồi ${data.kpis.collectionRate}%`;
    els.dueSoonCount.textContent = data.kpis.dueSoonCount;

    const maxAmount = Math.max(...data.statusBreakdown.map(x => x.amount), 1);
    els.statusBars.innerHTML = data.statusBreakdown.map(item => `
        <div class="bar-row">
            <strong>${item.status}</strong>
            <span class="bar-track"><span class="bar-fill" style="width:${Math.max(8, item.amount / maxAmount * 100)}%"></span></span>
            <span>${item.count} khoản</span>
        </div>
    `).join("");

    els.riskList.innerHTML = data.highRiskCustomers.map(item => `
        <div class="risk-item">
            <div>
                <strong>${item.name}</strong>
                <span class="sub">${item.phone}</span>
            </div>
            <span class="score">${item.riskScore}/100</span>
        </div>
    `).join("") || `<p>Không có khách hàng rủi ro cao.</p>`;

    els.alertList.innerHTML = data.upcomingAlerts.map(item => `
        <div class="timeline-item">
            <div>
                <strong>${item.name}</strong>
                <span class="sub">${money.format(item.remainingAmount)} · hạn ${dateFmt.format(new Date(item.dueDate))}</span>
            </div>
            <span class="badge ${item.status}">${item.status}</span>
        </div>
    `).join("") || `<p>Không có cảnh báo cần xử lý.</p>`;
}

function renderDebts(debts) {
    els.debtRows.innerHTML = debts.map(debt => `
        <tr>
            <td>
                <strong>${debt.customerName}</strong>
                <span class="sub">${debt.phone} · ${debt.address}</span>
            </td>
            <td>${debt.code}</td>
            <td>
                ${dateFmt.format(new Date(debt.dueDate))}
                <span class="sub">${debt.overdueDays > 0 ? `Quá hạn ${debt.overdueDays} ngày` : "Trong hạn"}</span>
            </td>
            <td>
                <strong>${money.format(debt.remainingAmount)}</strong>
                <span class="sub">Phạt ${money.format(debt.penaltyAmount)}</span>
            </td>
            <td><span class="score">${debt.riskScore}</span></td>
            <td><span class="badge ${debt.status}">${debt.status}</span></td>
            <td>
                <div class="row-actions">
                    <button class="secondary" onclick="payDebt('${debt.id}', ${Math.min(debt.remainingAmount, 1000000)})">Thu</button>
                    <button onclick="sendReminder('${debt.id}')">Nhắc</button>
                </div>
            </td>
        </tr>
    `).join("");
}

function renderNotifications(notifications) {
    els.notificationList.innerHTML = notifications.slice(0, 6).map(item => `
        <div class="timeline-item">
            <div>
                <strong>${item.channel} · ${item.status}</strong>
                <span class="sub">${item.message}</span>
            </div>
            <span>${dateFmt.format(new Date(item.sentAt))}</span>
        </div>
    `).join("") || `<p>Chưa có thông báo.</p>`;
}

async function loadDebts() {
    const params = new URLSearchParams();
    if (els.searchInput.value.trim()) params.set("search", els.searchInput.value.trim());
    if (els.statusFilter.value) params.set("status", els.statusFilter.value);
    const debts = await api(`/api/debts?${params}`);
    renderDebts(debts);
}

async function loadContracts() {
    const contracts = await api("/api/contracts");
    els.contractSelect.innerHTML = contracts.map(contract => `
        <option value="${contract.id}">${contract.code} · ${money.format(contract.amount)}</option>
    `).join("");
}

async function loadAll() {
    const [dashboard, notifications] = await Promise.all([
        api("/api/dashboard"),
        api("/api/notifications")
    ]);
    renderDashboard(dashboard);
    renderNotifications(notifications);
    await loadDebts();
}

async function payDebt(id, suggestedAmount) {
    const amountText = prompt("Nhập số tiền thu", String(Math.round(suggestedAmount)));
    if (!amountText) return;
    const amount = Number(amountText);
    if (!Number.isFinite(amount) || amount <= 0) return;

    await api(`/api/debts/${id}/payments`, {
        method: "POST",
        body: JSON.stringify({ amount, method: "Cash", receivedBy: "Operator" })
    });
    await loadAll();
}

async function sendReminder(id) {
    await api(`/api/debts/${id}/reminders`, {
        method: "POST",
        body: JSON.stringify({ channel: "SMS" })
    });
    await loadAll();
}

window.payDebt = payDebt;
window.sendReminder = sendReminder;

els.refreshBtn.addEventListener("click", loadAll);
els.searchInput.addEventListener("input", () => loadDebts().catch(console.error));
els.statusFilter.addEventListener("change", () => loadDebts().catch(console.error));
els.openCreateDebt.addEventListener("click", async () => {
    await loadContracts();
    const today = new Date();
    const due = new Date();
    due.setDate(today.getDate() + 14);
    els.issuedDateInput.value = toDateInput(today);
    els.dueDateInput.value = toDateInput(due);
    els.debtDialog.showModal();
});

els.createDebtBtn.addEventListener("click", async event => {
    event.preventDefault();
    await api("/api/debts", {
        method: "POST",
        body: JSON.stringify({
            contractId: els.contractSelect.value,
            principalAmount: Number(document.querySelector("#principalInput").value),
            penaltyRate: Number(document.querySelector("#penaltyRateInput").value),
            reminderFee: Number(document.querySelector("#reminderFeeInput").value),
            issuedDate: els.issuedDateInput.value,
            dueDate: els.dueDateInput.value,
            note: document.querySelector("#noteInput").value
        })
    });
    els.debtDialog.close();
    await loadAll();
});

if (localStorage.getItem("token")) {
    document.body.classList.remove("login-mode");
    document.getElementById("loginScreen").style.display = "none";

    loadAll().catch(error => {
        console.error(error);
        alert(error.message);
    });
}
function togglePassword() {
    const input = document.getElementById("passwordInput");

    input.type =
        input.type === "password"
            ? "text"
            : "password";
}

document
    .getElementById("loginBtn")
    .addEventListener("click", async () => {

        const email =
            document.getElementById("emailInput").value;

        const password =
            document.getElementById("passwordInput").value;

        const response = await fetch("/api/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email,
                password
            })
        });

        if (!response.ok) {
            alert("Đăng nhập thất bại");
            return;
        }

        const data = await response.json();

        localStorage.setItem(
            "token",
            data.accessToken
        );
        document.body.classList.remove("login-mode");

        document
            .getElementById("loginScreen")
            .style.display = "none";
        await loadAll();
    });

    document.getElementById("logoutBtn").addEventListener("click", () => {
    localStorage.clear();
    location.reload();
    });
