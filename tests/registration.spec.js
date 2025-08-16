const { test, expect } = require('@playwright/test');

test.describe('Registration Page Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to registration page
    await page.goto('/Identity/Register');
  });

  test('should load registration page successfully', async ({ page }) => {
    // Check page title
    await expect(page).toHaveTitle(/Register/);
    
    // Check main heading
    await expect(page.locator('h1.section-title')).toContainText('Register');
    
    // Check form exists
    await expect(page.locator('#registerForm')).toBeVisible();
  });

  test('should display all required form fields', async ({ page }) => {
    // Check all input fields are present
    await expect(page.locator('#firstName')).toBeVisible();
    await expect(page.locator('#lastName')).toBeVisible();
    await expect(page.locator('#email')).toBeVisible();
    await expect(page.locator('#phoneNumber')).toBeVisible();
    await expect(page.locator('#referralCode')).toBeVisible();
    await expect(page.locator('#password')).toBeVisible();
    await expect(page.locator('#confirmPassword')).toBeVisible();
    
    // Check consent checkboxes
    await expect(page.locator('#consent1')).toBeVisible();
    await expect(page.locator('#consent2')).toBeVisible();
    
    // Check submit button
    await expect(page.locator('#submitBtn')).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    // Try to submit empty form
    await page.click('#submitBtn');
    
    // Check that validation messages appear
    await expect(page.locator('.field-validation[data-field="firstName"]')).toContainText('First name is required');
    await expect(page.locator('.field-validation[data-field="lastName"]')).toContainText('Last name is required');
    await expect(page.locator('.field-validation[data-field="email"]')).toContainText('Email is required');
    await expect(page.locator('.field-validation[data-field="password"]')).toContainText('Password is required');
  });

  test('should validate email format', async ({ page }) => {
    // Enter invalid email
    await page.fill('#email', 'invalid-email');
    await page.click('#submitBtn');
    
    // Check email validation message
    await expect(page.locator('.field-validation[data-field="email"]')).toContainText('Please enter a valid email address');
  });

  test('should validate password confirmation', async ({ page }) => {
    // Enter different passwords
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'different123');
    await page.click('#submitBtn');
    
    // Check password confirmation validation
    await expect(page.locator('.field-validation[data-field="confirmPassword"]')).toContainText('Passwords do not match');
  });

  test('should validate minimum password length', async ({ page }) => {
    // Enter short password
    await page.fill('#password', '123');
    await page.click('#submitBtn');
    
    // Check password length validation
    await expect(page.locator('.field-validation[data-field="password"]')).toContainText('Password must be at least 6 characters');
  });

  test('should enable submit button only when consent boxes are checked', async ({ page }) => {
    // Initially submit button should be disabled (after first click to trigger validation)
    await page.click('#submitBtn');
    
    // Check consent1
    await page.check('#consent1');
    await expect(page.locator('#submitBtn')).toBeDisabled();
    
    // Check consent2
    await page.check('#consent2');
    await expect(page.locator('#submitBtn')).toBeEnabled();
    
    // Uncheck consent1
    await page.uncheck('#consent1');
    await expect(page.locator('#submitBtn')).toBeDisabled();
  });

  test('should handle referral code from URL parameter', async ({ page }) => {
    // Navigate with referral code
    await page.goto('/Identity/Register?referrerId=TEST123');
    
    // Check that referral code field is populated and readonly
    const referralField = page.locator('#referralCode');
    await expect(referralField).toHaveValue('TEST123');
    await expect(referralField).toHaveAttribute('readonly');
  });

  test('should allow manual entry of referral code when none provided', async ({ page }) => {
    // Referral code field should be editable
    const referralField = page.locator('#referralCode');
    await expect(referralField).not.toHaveAttribute('readonly');
    
    // Should be able to type in it
    await referralField.fill('MANUAL123');
    await expect(referralField).toHaveValue('MANUAL123');
  });

  test('should display loading state during form submission', async ({ page }) => {
    // Fill out form with valid data
    await page.fill('#firstName', 'John');
    await page.fill('#lastName', 'Doe');
    await page.fill('#email', 'john.doe@example.com');
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'password123');
    await page.check('#consent1');
    await page.check('#consent2');
    
    // Intercept the API call to delay it
    await page.route('/api/account/register', async route => {
      // Delay the response to see loading state
      await page.waitForTimeout(2000);
      await route.abort();
    });
    
    // Submit form
    await page.click('#submitBtn');
    
    // Check loading state
    await expect(page.locator('#btnSpinner')).toBeVisible();
    await expect(page.locator('#btnText')).toBeHidden();
    await expect(page.locator('#submitBtn')).toBeDisabled();
  });

  test('should handle API error responses gracefully', async ({ page }) => {
    // Fill out form with valid data
    await page.fill('#firstName', 'John');
    await page.fill('#lastName', 'Doe');
    await page.fill('#email', 'john.doe@example.com');
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'password123');
    await page.check('#consent1');
    await page.check('#consent2');
    
    // Mock API error response
    await page.route('/api/account/register', async route => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({
          success: false,
          errors: {
            Email: ['Email is already registered.']
          }
        })
      });
    });
    
    // Submit form
    await page.click('#submitBtn');
    
    // Check error message is displayed
    await expect(page.locator('.field-validation[data-field="email"]')).toContainText('Email is already registered.');
    
    // Check button is reset
    await expect(page.locator('#btnText')).toBeVisible();
    await expect(page.locator('#btnSpinner')).toBeHidden();
  });

  test('should handle successful registration', async ({ page }) => {
    // Fill out form with valid data
    await page.fill('#firstName', 'John');
    await page.fill('#lastName', 'Doe');
    await page.fill('#email', `test${Date.now()}@example.com`); // Unique email
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'password123');
    await page.check('#consent1');
    await page.check('#consent2');
    
    // Mock successful API response
    await page.route('/api/account/register', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          message: 'Registration successful!',
          redirectUrl: '/Membership/Dashboard/Index'
        })
      });
    });
    
    // Submit form
    await page.click('#submitBtn');
    
    // Check success message
    await expect(page.locator('#successMessage')).toBeVisible();
    await expect(page.locator('#successMessage')).toContainText('Registration successful!');
  });

  test('should test complete registration flow with referral code', async ({ page }) => {
    // Navigate with referral code
    await page.goto('/Identity/Register?referrerId=REFERRER123');
    
    // Fill out form
    await page.fill('#firstName', 'Jane');
    await page.fill('#lastName', 'Smith');
    await page.fill('#email', `jane${Date.now()}@example.com`);
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'password123');
    await page.check('#consent1');
    await page.check('#consent2');
    
    // Verify referral code is populated
    await expect(page.locator('#referralCode')).toHaveValue('REFERRER123');
    
    // Mock successful registration
    await page.route('/api/account/register', async route => {
      const request = route.request();
      const postData = request.postDataJSON();
      
      // Verify referral code is included in request
      expect(postData.referralCode).toBe('REFERRER123');
      
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          message: 'Registration successful!',
          redirectUrl: '/Membership/Dashboard/Index'
        })
      });
    });
    
    // Submit form
    await page.click('#submitBtn');
    
    // Check success
    await expect(page.locator('#successMessage')).toBeVisible();
  });

  test('should handle reCAPTCHA when configured', async ({ page }) => {
    // Mock reCAPTCHA
    await page.addInitScript(() => {
      window.grecaptcha = {
        ready: (callback) => callback(),
        execute: () => Promise.resolve('mock-recaptcha-token')
      };
    });
    
    // Set reCAPTCHA site key
    await page.evaluate(() => {
      document.querySelector('script[src*="recaptcha"]')?.setAttribute('data-sitekey', 'test-site-key');
    });
    
    // Fill out form
    await page.fill('#firstName', 'John');
    await page.fill('#lastName', 'Doe');
    await page.fill('#email', `john${Date.now()}@example.com`);
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'password123');
    await page.check('#consent1');
    await page.check('#consent2');
    
    // Mock API call to verify reCAPTCHA token is sent
    await page.route('/api/account/register', async route => {
      const request = route.request();
      const postData = request.postDataJSON();
      
      // Verify reCAPTCHA token is included
      expect(postData.recaptchaToken).toBe('mock-recaptcha-token');
      
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          success: true,
          message: 'Registration successful!',
          redirectUrl: '/Membership/Dashboard/Index'
        })
      });
    });
    
    // Submit form
    await page.click('#submitBtn');
    
    // Verify token was set
    await expect(page.locator('#recaptchaToken')).toHaveValue('mock-recaptcha-token');
  });

  test('should be responsive on mobile devices', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    // Check form is still visible and usable
    await expect(page.locator('#registerForm')).toBeVisible();
    await expect(page.locator('#firstName')).toBeVisible();
    await expect(page.locator('#submitBtn')).toBeVisible();
    
    // Test form interaction on mobile
    await page.fill('#firstName', 'Mobile');
    await page.fill('#lastName', 'User');
    await expect(page.locator('#firstName')).toHaveValue('Mobile');
  });
});