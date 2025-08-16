const { test, expect } = require('@playwright/test');

test.describe('Simple Registration Tests', () => {
  test('can load registration page', async ({ page }) => {
    try {
      await page.goto('https://localhost:44342/Identity/Register', { 
        waitUntil: 'networkidle',
        timeout: 30000 
      });
      
      // Take a screenshot for debugging
      await page.screenshot({ path: 'registration-page-load.png' });
      
      // Check if page loaded
      const title = await page.title();
      console.log('Page title:', title);
      
      // Check if form exists
      const form = await page.locator('#registerForm');
      const formExists = await form.count();
      console.log('Form exists:', formExists > 0);
      
      if (formExists === 0) {
        // Look for any error messages
        const errorElements = await page.locator('.alert-danger').count();
        console.log('Error elements found:', errorElements);
        
        if (errorElements > 0) {
          const errorText = await page.locator('.alert-danger').first().textContent();
          console.log('Error message:', errorText);
        }
      }
      
      // Check console errors
      page.on('console', msg => {
        if (msg.type() === 'error') {
          console.log('Console error:', msg.text());
        }
      });
      
      expect(formExists).toBeGreaterThan(0);
      
    } catch (error) {
      console.error('Test failed:', error.message);
      await page.screenshot({ path: 'registration-error.png', fullPage: true });
      throw error;
    }
  });

  test('check form fields exist', async ({ page }) => {
    await page.goto('https://localhost:44342/Identity/Register', { 
      waitUntil: 'networkidle',
      timeout: 30000 
    });
    
    // Check each field individually
    const fields = [
      '#firstName',
      '#lastName', 
      '#email',
      '#phoneNumber',
      '#referralCode',
      '#password',
      '#confirmPassword',
      '#consent1',
      '#consent2',
      '#submitBtn'
    ];
    
    for (const field of fields) {
      const element = page.locator(field);
      const exists = await element.count();
      console.log(`Field ${field} exists:`, exists > 0);
      
      if (exists === 0) {
        // Take screenshot of issue
        await page.screenshot({ path: `missing-field-${field.replace('#', '')}.png` });
      }
    }
  });

  test('test form validation', async ({ page }) => {
    await page.goto('https://localhost:44342/Identity/Register', { 
      waitUntil: 'networkidle',
      timeout: 30000 
    });
    
    // Try to submit empty form
    const submitBtn = page.locator('#submitBtn');
    
    if (await submitBtn.count() > 0) {
      await submitBtn.click();
      
      // Wait a bit for validation
      await page.waitForTimeout(2000);
      
      // Check for validation messages
      const validationElements = await page.locator('.field-validation').count();
      console.log('Validation elements found:', validationElements);
      
      // Take screenshot
      await page.screenshot({ path: 'form-validation.png' });
      
      // Log any validation messages
      const validations = await page.locator('.field-validation').all();
      for (let i = 0; i < validations.length; i++) {
        const text = await validations[i].textContent();
        if (text && text.trim()) {
          console.log(`Validation ${i}:`, text.trim());
        }
      }
    }
  });

  test('test AJAX submission', async ({ page }) => {
    await page.goto('https://localhost:44342/Identity/Register', { 
      waitUntil: 'networkidle',
      timeout: 30000 
    });
    
    // Fill form with test data
    await page.fill('#firstName', 'Test');
    await page.fill('#lastName', 'User');
    await page.fill('#email', 'test@example.com');
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'password123');
    
    // Check consent boxes
    await page.check('#consent1');
    await page.check('#consent2');
    
    // Monitor network requests
    const requests = [];
    page.on('request', request => {
      if (request.url().includes('/api/account/register')) {
        requests.push(request);
        console.log('API request made:', request.url());
      }
    });
    
    // Monitor responses
    page.on('response', response => {
      if (response.url().includes('/api/account/register')) {
        console.log('API response:', response.status());
      }
    });
    
    // Submit form
    await page.click('#submitBtn');
    
    // Wait for potential AJAX call
    await page.waitForTimeout(5000);
    
    console.log('Total API requests made:', requests.length);
    
    // Take screenshot of result
    await page.screenshot({ path: 'ajax-submission.png' });
  });
});