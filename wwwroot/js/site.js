// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// site.js - Common JavaScript functionality for SteadyGrowth

// AJAX helper
window.ajax = function (options) {
    return $.ajax(options);
};

// Form validation enhancements
$(function () {
    $("form[data-validate='true']").each(function () {
        $(this).validate();
    });
});

// Loading state management
window.showLoading = function (selector) {
    $(selector).addClass('loading');
};
window.hideLoading = function (selector) {
    $(selector).removeClass('loading');
};

// Toast notifications
window.showToast = function (message, type = 'info') {
    const toast = $(
        `<div class="toast align-items-center text-bg-${type} border-0 show" role="alert" aria-live="assertive" aria-atomic="true" style="position: fixed; top: 1rem; right: 1rem; z-index: 9999;">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`
    );
    $("body").append(toast);
    setTimeout(() => toast.fadeOut(400, () => toast.remove()), 4000);
};
