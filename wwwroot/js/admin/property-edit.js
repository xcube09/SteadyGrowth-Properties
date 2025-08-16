// Property Edit JavaScript - Enhanced UX
"use strict";

var PropertyEdit = function () {
    var stepper;
    var form;
    var submitButton;
    var nextButton;
    var previousButton;
    var dropzone;
    var unsavedChanges = false;
    var autoSaveInterval;
    var imagesToDelete = [];
    var newImages = [];

    var initStepper = function () {
        const stepperElement = document.querySelector("#kt_property_edit_stepper");
        if (!stepperElement) {
            console.error('Stepper element not found');
            return;
        }

        // Check if KTStepper is available
        if (typeof KTStepper === 'undefined') {
            console.warn('KTStepper not available, using fallback');
            initSimpleStepper();
            return;
        }

        try {
            // Initialize stepper
            stepper = new KTStepper(stepperElement);

            // Handle next step
            stepper.on("kt.stepper.next", function (stepper) {
                if (validateCurrentStep()) {
                    stepper.goNext();
                    if (typeof KTUtil !== 'undefined' && KTUtil.scrollTop) {
                        KTUtil.scrollTop();
                    } else {
                        window.scrollTo(0, 0);
                    }
                    updateButtons();
                }
            });

            // Handle previous step
            stepper.on("kt.stepper.previous", function (stepper) {
                stepper.goPrevious();
                if (typeof KTUtil !== 'undefined' && KTUtil.scrollTop) {
                    KTUtil.scrollTop();
                } else {
                    window.scrollTo(0, 0);
                }
                updateButtons();
            });

            // Update buttons on stepper change
            stepper.on("kt.stepper.changed", function () {
                updateButtons();
                if (stepper.getCurrentStepIndex() === 4) {
                    populateReviewSummary();
                }
            });
            
            // Initial button update after KTStepper is ready
            setTimeout(() => updateButtons(), 100);
        } catch (error) {
            console.error('Error initializing KTStepper:', error);
            initSimpleStepper();
        }
    };

    var initSimpleStepper = function () {
        console.log('Initializing simple stepper fallback');
        
        // Simple fallback stepper implementation
        let currentStepIndex = 1;
        const totalSteps = 4;
        
        stepper = {
            getCurrentStepIndex: () => currentStepIndex,
            getTotalStepsNumber: () => totalSteps,
            goNext: () => {
                if (currentStepIndex < totalSteps) {
                    hideCurrentStep();
                    currentStepIndex++;
                    showCurrentStep();
                    updateStepperUI();
                }
            },
            goPrevious: () => {
                if (currentStepIndex > 1) {
                    hideCurrentStep();
                    currentStepIndex--;
                    showCurrentStep();
                    updateStepperUI();
                }
            }
        };

        function hideCurrentStep() {
            document.querySelectorAll('[data-kt-stepper-element="content"]').forEach(el => {
                el.style.display = 'none';
            });
            document.querySelectorAll('.stepper-item').forEach(el => {
                el.classList.remove('current');
            });
        }

        function showCurrentStep() {
            const contentElements = document.querySelectorAll('[data-kt-stepper-element="content"]');
            if (contentElements[currentStepIndex - 1]) {
                contentElements[currentStepIndex - 1].style.display = 'block';
            }
            
            const stepElements = document.querySelectorAll('.stepper-item');
            if (stepElements[currentStepIndex - 1]) {
                stepElements[currentStepIndex - 1].classList.add('current');
            }
        }

        function updateStepperUI() {
            updateButtons();
            if (currentStepIndex === 4) {
                populateReviewSummary();
            }
        }

        // Initialize first step
        showCurrentStep();
        // Defer updateButtons to ensure it's called after stepper is ready
        setTimeout(() => updateButtons(), 100);
    };

    var initForm = function () {
        form = document.getElementById('property-edit-form');
        submitButton = document.querySelector('[data-kt-stepper-action="submit"]');
        nextButton = document.querySelector('[data-kt-stepper-action="next"]');
        previousButton = document.querySelector('[data-kt-stepper-action="previous"]');

        // Handle form input changes
        $(form).on('input change', function () {
            markAsChanged();
            clearAutoSave();
            scheduleAutoSave();
        });

        // Handle form submission
        submitButton.addEventListener('click', function (e) {
            e.preventDefault();
            submitForm();
        });

        // Handle next button
        nextButton.addEventListener('click', function (e) {
            e.preventDefault();
            stepper.goNext();
        });

        // Handle previous button
        previousButton.addEventListener('click', function (e) {
            e.preventDefault();
            stepper.goPrevious();
        });
    };

    var initDropzone = function () {
        const dropzoneElement = document.querySelector("#property-image-dropzone");
        if (!dropzoneElement) {
            console.log('Dropzone element not found');
            return;
        }

        // Check if Dropzone is available
        if (typeof Dropzone === 'undefined') {
            console.warn('Dropzone not available, setting up simple file input');
            setupSimpleFileUpload(dropzoneElement);
            return;
        }

        try {
            // Disable auto-discover
            Dropzone.autoDiscover = false;

            dropzone = new Dropzone(dropzoneElement, {
                url: "/Admin/Properties/Edit?handler=UploadImage",
                paramName: "file",
                maxFiles: 10,
                maxFilesize: 50, // MB
                acceptedFiles: ".jpeg,.jpg,.png,.gif",
                addRemoveLinks: true,
                dictDefaultMessage: "Drop files here or click to upload",
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                }
            });
        } catch (error) {
            console.error('Error initializing Dropzone:', error);
            setupSimpleFileUpload(dropzoneElement);
        }
    };

    var setupSimpleFileUpload = function (container) {
        console.log('Setting up simple file upload');
        
        const fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.multiple = true;
        fileInput.accept = '.jpeg,.jpg,.png,.gif';
        fileInput.className = 'form-control';
        fileInput.style.marginTop = '10px';
        
        const label = document.createElement('label');
        label.textContent = 'Select images to upload';
        label.className = 'form-label';
        
        // Clear the container and add simple upload
        container.innerHTML = '';
        container.appendChild(label);
        container.appendChild(fileInput);
        
        fileInput.addEventListener('change', function(e) {
            const files = e.target.files;
            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                console.log('File selected:', file.name);
                
                // Add to new images array (simplified)
                newImages.push({
                    fileName: file.name,
                    originalName: file.name,
                    caption: '',
                    imageType: '',
                    displayOrder: getNextDisplayOrder()
                });
            }
            
            if (files.length > 0) {
                showNotification('success', `${files.length} file(s) selected`);
                markAsChanged();
            }
        });
        
        // Add dropzone event handlers only if dropzone was successfully initialized
        if (dropzone && typeof dropzone.on === 'function') {
            dropzone.on("success", function (file, response) {
                if (response.success) {
                    // Add to new images array
                    newImages.push({
                        fileName: response.fileName,
                        originalName: response.originalName,
                        caption: '',
                        imageType: '',
                        displayOrder: getNextDisplayOrder()
                    });

                    // Show success notification
                    showNotification('success', 'Image uploaded successfully');
                    markAsChanged();
                } else {
                    // Show error
                    showNotification('error', response.message);
                    dropzone.removeFile(file);
                }
            });

            dropzone.on("error", function (file, errorMessage) {
                showNotification('error', 'Upload Error: ' + errorMessage);
            });
        }
    };

    var initImageHandlers = function () {
        // Handle image deletion
        $(document).on('click', '.delete-image-btn', function (e) {
            e.preventDefault();
            const imageId = $(this).data('image-id');
            const imageCard = $(this).closest('[data-image-id]');

            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    title: 'Delete Image?',
                    text: 'This action cannot be undone.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6',
                    confirmButtonText: 'Yes, delete it!'
                }).then((result) => {
                    if (result.isConfirmed) {
                        handleImageDeletion(imageId, imageCard);
                    }
                });
            } else {
                // Fallback confirmation
                if (confirm('Delete this image? This action cannot be undone.')) {
                    handleImageDeletion(imageId, imageCard);
                }
            }
        });
    };

    var handleImageDeletion = function (imageId, imageCard) {
        // Add to deletion list
        imagesToDelete.push(imageId);
        
        // Remove from UI with animation
        imageCard.fadeOut(300, function() {
            $(this).remove();
        });

        markAsChanged();
        showNotification('success', 'Image marked for deletion');
    };

    var validateCurrentStep = function () {
        const currentStep = stepper.getCurrentStepIndex();
        let isValid = true;

        switch (currentStep) {
            case 1:
                // Validate basic info
                const title = document.querySelector('#Command_Title').value.trim();
                const description = document.querySelector('#Command_Description').value.trim();
                const price = document.querySelector('#Command_Price').value;
                const location = document.querySelector('#Command_Location').value.trim();

                if (!title || !description || !price || !location) {
                    showValidationError('Please fill in all required fields.');
                    isValid = false;
                }
                break;

            case 2:
                // Validate details - minimal validation for now
                break;

            case 3:
                // Validate images - optional for update
                break;
        }

        return isValid;
    };

    var updateButtons = function () {
        // Safety check - ensure stepper is initialized
        if (!stepper || typeof stepper.getCurrentStepIndex !== 'function' || typeof stepper.getTotalStepsNumber !== 'function') {
            console.log('⚠️ Stepper not fully initialized yet, skipping updateButtons');
            return;
        }
        
        const currentStep = stepper.getCurrentStepIndex();
        const totalSteps = stepper.getTotalStepsNumber();

        // Show/hide previous button
        if (currentStep === 1) {
            previousButton.style.display = 'none';
        } else {
            previousButton.style.display = 'inline-block';
        }

        // Show/hide next vs submit button
        if (currentStep === totalSteps) {
            nextButton.style.display = 'none';
            submitButton.style.display = 'inline-block';
        } else {
            nextButton.style.display = 'inline-block';
            submitButton.style.display = 'none';
        }
    };

    var populateReviewSummary = function () {
        const reviewContainer = document.getElementById('review-summary');
        const formData = new FormData(form);

        let html = '<div class="row">';
        
        // Basic Information
        html += '<div class="col-md-6 mb-10">';
        html += '<div class="card card-flush border-0 h-md-50">';
        html += '<div class="card-header pt-5"><h3 class="card-title text-gray-800 fw-bold">Basic Information</h3></div>';
        html += '<div class="card-body pt-0">';
        html += `<div class="mb-3"><strong>Title:</strong> ${formData.get('Command.Title')}</div>`;
        html += `<div class="mb-3"><strong>Price:</strong> ₦${parseFloat(formData.get('Command.Price')).toLocaleString()}</div>`;
        html += `<div class="mb-3"><strong>Location:</strong> ${formData.get('Command.Location')}</div>`;
        html += `<div><strong>Description:</strong> ${formData.get('Command.Description').substring(0, 100)}...</div>`;
        html += '</div></div></div>';

        // Property Details
        html += '<div class="col-md-6 mb-10">';
        html += '<div class="card card-flush border-0 h-md-50">';
        html += '<div class="card-header pt-5"><h3 class="card-title text-gray-800 fw-bold">Property Details</h3></div>';
        html += '<div class="card-body pt-0">';
        
        const propertyTypes = ['Residential', 'Commercial', 'Land', 'Mixed'];
        const statusTypes = ['Draft', 'Pending', 'Approved', 'Rejected'];
        
        html += `<div class="mb-3"><strong>Type:</strong> ${propertyTypes[parseInt(formData.get('Command.PropertyType'))]}</div>`;
        html += `<div class="mb-3"><strong>Status:</strong> ${statusTypes[parseInt(formData.get('Command.Status'))]}</div>`;
        html += `<div><strong>Active:</strong> ${formData.get('Command.IsActive') ? 'Yes' : 'No'}</div>`;
        html += '</div></div></div>';

        html += '</div>';

        // Image Summary
        const totalExistingImages = document.querySelectorAll('[data-image-id]').length - imagesToDelete.length;
        const totalNewImages = newImages.length;
        const totalImages = totalExistingImages + totalNewImages;

        html += '<div class="card card-flush border-0 mt-5">';
        html += '<div class="card-header pt-5"><h3 class="card-title text-gray-800 fw-bold">Images Summary</h3></div>';
        html += '<div class="card-body pt-0">';
        html += `<div class="mb-3"><strong>Total Images:</strong> ${totalImages}</div>`;
        html += `<div class="mb-3"><strong>New Images:</strong> ${totalNewImages}</div>`;
        html += `<div><strong>Images to Delete:</strong> ${imagesToDelete.length}</div>`;
        html += '</div></div>';

        reviewContainer.innerHTML = html;
    };

    var submitForm = function () {
        if (!validateCurrentStep()) {
            return;
        }

        // Show loading state
        submitButton.setAttribute("data-kt-indicator", "on");
        submitButton.disabled = true;

        // Add hidden inputs for image operations
        addHiddenImageInputs();

        // Submit form
        form.submit();
    };

    var addHiddenImageInputs = function () {
        // Remove any existing hidden inputs
        $('.property-edit-hidden-input').remove();

        // Add images to delete
        imagesToDelete.forEach((imageId, index) => {
            const input = $(`<input type="hidden" name="Command.ImagesToDelete[${index}]" value="${imageId}" class="property-edit-hidden-input" />`);
            $(form).append(input);
        });

        // Add new images
        newImages.forEach((image, index) => {
            const baseInputName = `Command.NewImages[${index}]`;
            $(form).append(`<input type="hidden" name="${baseInputName}.FileName" value="${image.fileName}" class="property-edit-hidden-input" />`);
            $(form).append(`<input type="hidden" name="${baseInputName}.Caption" value="${image.caption}" class="property-edit-hidden-input" />`);
            $(form).append(`<input type="hidden" name="${baseInputName}.ImageType" value="${image.imageType}" class="property-edit-hidden-input" />`);
            $(form).append(`<input type="hidden" name="${baseInputName}.DisplayOrder" value="${image.displayOrder}" class="property-edit-hidden-input" />`);
        });
    };

    var markAsChanged = function () {
        unsavedChanges = true;
        // Add visual indicator
        if (!$('.unsaved-indicator').length) {
            $('.card-title h3').append('<span class="unsaved-indicator badge badge-warning ms-2">Unsaved</span>');
        }
    };

    var markAsSaved = function () {
        unsavedChanges = false;
        $('.unsaved-indicator').remove();
    };

    var scheduleAutoSave = function () {
        clearAutoSave();
        autoSaveInterval = setTimeout(function () {
            // Implement auto-save functionality
            console.log('Auto-saving...');
            // You can implement AJAX auto-save here
        }, 30000); // 30 seconds
    };

    var clearAutoSave = function () {
        if (autoSaveInterval) {
            clearTimeout(autoSaveInterval);
        }
    };

    var getNextDisplayOrder = function () {
        const existingOrders = [];
        document.querySelectorAll('[data-image-id]').forEach(el => {
            const order = parseInt(el.querySelector('.fs-4').textContent.replace('Image ', ''));
            existingOrders.push(order);
        });
        newImages.forEach(img => existingOrders.push(img.displayOrder));
        
        return Math.max(0, ...existingOrders) + 1;
    };

    var showValidationError = function (message) {
        showNotification('error', message);
    };

    var showNotification = function (type, message) {
        if (typeof Swal !== 'undefined') {
            if (type === 'success') {
                Swal.fire({
                    toast: true,
                    position: 'top-end',
                    icon: 'success',
                    title: message,
                    showConfirmButton: false,
                    timer: 3000
                });
            } else {
                Swal.fire({
                    title: type === 'error' ? 'Error' : 'Info',
                    text: message,
                    icon: type,
                    confirmButtonText: 'OK'
                });
            }
        } else {
            // Fallback to console and alert
            console.log(`${type.toUpperCase()}: ${message}`);
            alert(message);
        }
    };

    var initUnloadWarning = function () {
        window.addEventListener('beforeunload', function (e) {
            if (unsavedChanges) {
                e.preventDefault();
                e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
                return e.returnValue;
            }
        });
    };

    return {
        init: function () {
            initStepper();
            initForm();
            initDropzone();
            initImageHandlers();
            initUnloadWarning();

            // Initialize tooltips
            $('[data-bs-toggle="tooltip"]').tooltip();

            console.log('Property Edit initialized');
        }
    };
}();

// Initialize when document is ready
$(document).ready(function () {
    PropertyEdit.init();
});