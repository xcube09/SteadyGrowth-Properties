const { test, expect } = require('@playwright/test');

test.describe('Registration Page Debug Tests', () => {
  test('Check if registration page loads and identify issues', async ({ page }) => {
    console.log('Starting registration page debug test...');
    
    // Navigate to the registration page
    await page.goto('/Identity/Register');
    
    // Wait for page to load
    await page.waitForLoadState('networkidle');
    
    // Check if the page loads successfully
    await expect(page).toHaveTitle(/Register/);
    console.log('✓ Registration page loaded successfully');
    
    // Check if the form exists
    const form = page.locator('#registerForm');
    await expect(form).toBeVisible();
    console.log('✓ Registration form is visible');
    
    // Check if all required fields exist
    const fields = [
      '#firstName',
      '#lastName', 
      '#email',
      '#phoneNumber',
      '#referralCode',
      '#password',
      '#confirmPassword'
    ];
    
    for (const field of fields) {
      await expect(page.locator(field)).toBeVisible();
      console.log(`✓ Field ${field} is visible`);
    }
    
    // Check if jQuery is loaded
    const jqueryLoaded = await page.evaluate(() => {
      return typeof window.$ !== 'undefined' && typeof window.jQuery !== 'undefined';
    });
    console.log(`jQuery loaded: ${jqueryLoaded ? '✓' : '✗'}`);
    
    // Check if jQuery validation is loaded
    const validationLoaded = await page.evaluate(() => {
      return typeof window.$.validator !== 'undefined';
    });
    console.log(`jQuery Validation loaded: ${validationLoaded ? '✓' : '✗'}`);
    
    // Check for JavaScript errors in console
    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    
    // Try to submit empty form to test validation
    await page.click('#submitBtn');
    await page.waitForTimeout(2000);
    
    // Check if validation errors appear
    const validationErrors = await page.locator('.field-validation').allTextContents();
    console.log('Validation errors found:', validationErrors.filter(text => text.trim()));
    
    // Check if consent checkboxes work
    await page.check('#consent1');
    await page.check('#consent2');
    
    const submitEnabled = await page.locator('#submitBtn').isEnabled();
    console.log(`Submit button enabled after consents: ${submitEnabled ? '✓' : '✗'}`);
    
    // Fill form with test data
    await page.fill('#firstName', 'Test');
    await page.fill('#lastName', 'User');
    await page.fill('#email', 'test@example.com');
    await page.fill('#password', 'Password123!');
    await page.fill('#confirmPassword', 'Password123!');
    
    // Try to submit and see what happens
    console.log('Attempting form submission...');
    await page.click('#submitBtn');
    
    // Wait for any response
    await page.waitForTimeout(5000);
    
    // Check for success/error messages
    const successMsg = await page.locator('#successMessage').isVisible();
    const errorMsg = await page.locator('#generalError').isVisible();
    
    console.log(`Success message visible: ${successMsg ? '✓' : '✗'}`);
    console.log(`Error message visible: ${errorMsg ? '✓' : '✗'}`);
    
    if (errorMsg) {
      const errorText = await page.locator('#generalError').textContent();
      console.log('Error message:', errorText);
    }
    
    // Check network requests
    console.log('Console errors found:', consoleErrors);
    
    // Check if reCAPTCHA is configured
    const recaptchaKey = await page.locator('script[src*="recaptcha"]').count();
    console.log(`reCAPTCHA script loaded: ${recaptchaKey > 0 ? '✓' : '✗'}`);
    
    // Take a screenshot for debugging
    await page.screenshot({ path: 'tests/debug-registration-page.png' });
    console.log('Screenshot saved to tests/debug-registration-page.png');
  });
  
  test('Test API endpoint directly', async ({ request }) => {
    console.log('Testing registration API endpoint...');
    
    const testData = {
      firstName: 'Test',
      lastName: 'User',
      email: 'test@example.com',
      phoneNumber: '1234567890',
      referralCode: '',
      password: 'Password123!',
      confirmPassword: 'Password123!',
      recaptchaToken: ''
    };
    
    try {
      const response = await request.post('/api/account/register', {
        data: testData,
        headers: {
          'Content-Type': 'application/json'
        }
      });
      
      console.log(`API Response Status: ${response.status()}`);
      
      if (response.ok()) {
        const data = await response.json();
        console.log('API Response Data:', data);
      } else {
        const errorData = await response.text();
        console.log('API Error Response:', errorData);
      }
    } catch (error) {
      console.log('API Request Error:', error.message);
    }
  });
});