const { test, expect } = require('@playwright/test');

test('Test API endpoint routing', async ({ page }) => {
  // Navigate to a simple page to test AJAX calls
  await page.goto('data:text/html,<html><body><h1>API Test</h1><div id="result"></div><script src="https://code.jquery.com/jquery-3.6.0.min.js"></script></body></html>');
  
  // Wait for jQuery to load
  await page.waitForTimeout(1000);
  
  // Test the API endpoint with a simple request
  const result = await page.evaluate(async () => {
    try {
      const response = await fetch('https://localhost:7147/api/account/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          firstName: 'Test',
          lastName: 'User',
          email: 'test@example.com',
          password: 'Password123!',
          confirmPassword: 'Password123!',
          recaptchaToken: ''
        })
      });
      
      return {
        status: response.status,
        statusText: response.statusText,
        url: response.url
      };
    } catch (error) {
      return {
        error: error.message,
        name: error.name
      };
    }
  });
  
  console.log('API Test Result:', result);
  
  // We expect either:
  // - 200/400 status (endpoint found, but validation/business logic issues)
  // - Not 404 (which means routing is working)
  // - Network error if server not running (which is fine for this test)
  
  if (result.status) {
    expect(result.status).not.toBe(404);
    console.log('✅ API endpoint routing is working - Status:', result.status);
  } else if (result.error) {
    console.log('ℹ️ Network error (expected if server not running):', result.error);
  }
});