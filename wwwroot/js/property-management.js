// property-management.js - Property creation/editing functionality

// Image upload and preview
$(function () {
    $(".property-image-input").on('change', function () {
        const input = this;
        if (input.files && input.files[0]) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $(input).closest('.image-upload-group').find('.image-preview').attr('src', e.target.result).show();
            };
            reader.readAsDataURL(input.files[0]);
        }
    });
});

// Form step navigation
window.gotoFormStep = function (step) {
    $(".form-step").hide();
    $("#form-step-" + step).show();
};

// Validation and submission
window.submitPropertyForm = function (formSelector) {
    const $form = $(formSelector);
    if ($form.valid()) {
        $form.submit();
    } else {
        window.showToast('Please fix validation errors.', 'danger');
    }
};
