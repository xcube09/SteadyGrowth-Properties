(function() {
    'use strict';
    
    // Email decoder function
    function decodeEmail(encoded) {
        // Simple character substitution decoder
        return encoded
            .replace(/\[at\]/g, '@')
            .replace(/\[dot\]/g, '.')
            .replace(/\s+/g, '');
    }
    
    // Initialize email protection
    function initEmailProtection() {
        // Find all protected email elements
        const protectedEmails = document.querySelectorAll('[data-email-protected]');
        
        protectedEmails.forEach(function(element) {
            const encodedEmail = element.getAttribute('data-email-protected');
            const decodedEmail = decodeEmail(encodedEmail);
            
            // Check if this should be a mailto link
            if (element.hasAttribute('data-email-link')) {
                // Create mailto link
                const link = document.createElement('a');
                link.href = 'mailto:' + decodedEmail;
                link.innerHTML = '<i class="icon-mail"></i> ' + obfuscateDisplay(decodedEmail);
                
                // Replace the element
                element.parentNode.replaceChild(link, element);
            } else {
                // Just display the email
                element.innerHTML = obfuscateDisplay(decodedEmail);
            }
        });
    }
    
    // Obfuscate email display using HTML entities and zero-width spaces
    function obfuscateDisplay(email) {
        // Split email into parts
        const parts = email.split('@');
        if (parts.length !== 2) return email;
        
        const localPart = parts[0];
        const domainParts = parts[1].split('.');
        
        // Build obfuscated display with HTML entities
        let obfuscated = '';
        
        // Add local part with zero-width spaces
        for (let i = 0; i < localPart.length; i++) {
            obfuscated += localPart[i];
            if (i < localPart.length - 1 && i % 2 === 0) {
                obfuscated += '&#8203;'; // Zero-width space
            }
        }
        
        // Add @ symbol as HTML entity
        obfuscated += '&#64;';
        
        // Add domain with zero-width spaces
        obfuscated += domainParts[0];
        obfuscated += '&#46;'; // Period as HTML entity
        
        for (let i = 1; i < domainParts.length; i++) {
            if (i > 1) obfuscated += '&#46;';
            obfuscated += domainParts[i];
        }
        
        return obfuscated;
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initEmailProtection);
    } else {
        initEmailProtection();
    }
    
    // Also handle dynamic content
    window.initEmailProtection = initEmailProtection;
})();