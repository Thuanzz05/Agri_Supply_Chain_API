var current_url = "http://localhost:5041";

// API Endpoints
var API = {
    // Auth
    login: current_url + "/api/login",
    
    // Nông Dân Service
    nongdan: {
        getAll: current_url + "/api-nongdan/nong-dan/get-all",
        getById: (id) => current_url + `/api-nongdan/nong-dan/get-by-id/${id}`,
        create: current_url + "/api-nongdan/nong-dan/create",
        update: (id) => current_url + `/api-nongdan/nong-dan/update/${id}`,
        delete: (id) => current_url + `/api-nongdan/nong-dan/delete/${id}`
    },
    
    // Đại Lý Service
    daily: {
        getAll: current_url + "/api-daily/dai-ly/get-all",
        getById: (id) => current_url + `/api-daily/dai-ly/get-by-id/${id}`,
        create: current_url + "/api-daily/dai-ly/create",
        update: (id) => current_url + `/api-daily/dai-ly/update/${id}`,
        delete: (id) => current_url + `/api-daily/dai-ly/delete/${id}`
    },
    
    // Kho (Đại Lý)
    kho: {
        getAll: current_url + "/api-daily/kho/get-all",
        getById: (id) => current_url + `/api-daily/kho/get-by-id/${id}`,
        getByMaDaiLy: (id) => current_url + `/api-daily/kho/get-by-ma-dai-ly/${id}`,
        create: current_url + "/api-daily/kho/create",
        update: (id) => current_url + `/api-daily/kho/update/${id}`,
        delete: (id) => current_url + `/api-daily/kho/delete/${id}`
    },
    
    // Kiểm Định (Đại Lý)
    kiemdinh: {
        getAll: current_url + "/api-daily/kiem-dinh/get-all",
        getById: (id) => current_url + `/api-daily/kiem-dinh/get-by-id/${id}`,
        create: current_url + "/api-daily/kiem-dinh/create",
        update: (id) => current_url + `/api-daily/kiem-dinh/update/${id}`,
        delete: (id) => current_url + `/api-daily/kiem-dinh/delete/${id}`
    },
    
    // Đơn Hàng Đại Lý
    donHangDaiLy: {
        getAll: current_url + "/api-daily/don-hang-dai-ly/get-all",
        getById: (id) => current_url + `/api-daily/don-hang-dai-ly/get-by-id/${id}`,
        getByMaDaiLy: (id) => current_url + `/api-daily/don-hang-dai-ly/get-by-ma-dai-ly/${id}`,
        create: current_url + "/api-daily/don-hang-dai-ly/create",
        updateTrangThai: (id) => current_url + `/api-daily/don-hang-dai-ly/update-trang-thai/${id}`,
        delete: (id) => current_url + `/api-daily/don-hang-dai-ly/delete/${id}`
    },
    
    // Đơn Hàng Siêu Thị (view từ Đại Lý)
    donHangSieuThiDaily: {
        getAll: current_url + "/api-daily/don-hang-sieu-thi/get-all",
        getById: (id) => current_url + `/api-daily/don-hang-sieu-thi/get-by-id/${id}`,
        getByMaDaiLy: (id) => current_url + `/api-daily/don-hang-sieu-thi/get-by-ma-dai-ly/${id}`,
        updateTrangThai: (id) => current_url + `/api-daily/don-hang-sieu-thi/update-trang-thai/${id}`
    },
    
    // Siêu Thị Service
    sieuthi: {
        getAll: current_url + "/api-sieuthi/sieu-thi/get-all",
        getById: (id) => current_url + `/api-sieuthi/sieu-thi/get-by-id/${id}`,
        create: current_url + "/api-sieuthi/sieu-thi/create",
        update: (id) => current_url + `/api-sieuthi/sieu-thi/update/${id}`,
        delete: (id) => current_url + `/api-sieuthi/sieu-thi/delete/${id}`
    },
    
    // Admin Service
    admin: {
        getAll: current_url + "/api-admin/admin/get-all",
        getById: (id) => current_url + `/api-admin/admin/get-by-id/${id}`,
        create: current_url + "/api-admin/admin/create",
        update: (id) => current_url + `/api-admin/admin/update/${id}`,
        delete: (id) => current_url + `/api-admin/admin/delete/${id}`
    }
};

// Helper function để inject script
makeScript = function (url) {
    var script = document.createElement('script');
    script.setAttribute('src', url);
    script.setAttribute('type', 'text/javascript');
    var mainDiv = document.getElementById('mainDiv');
    if (mainDiv) {
        mainDiv.appendChild(script);
    } else {
        document.body.appendChild(script);
    }
};

// Helper function để lấy token từ localStorage
function getToken() {
    var user = JSON.parse(localStorage.getItem('currentUser') || '{}');
    return user.Token || '';
}

// Helper function để tạo headers với token
function getAuthHeaders() {
    return {
        'Authorization': 'Bearer ' + getToken(),
        'Content-Type': 'application/json'
    };
}
