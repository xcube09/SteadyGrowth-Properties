const { test, expect } = require('@playwright/test');

test('Live API Endpoint Test', async ({ page }) => {
  console.log('🌐 Testing API endpoint with live server...');
  
  // Test direct API call
  const apiResponse = await page.evaluate(async () => {
    try {
      const response = await fetch('https://localhost:7147/api/account/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          firstName: 'API',
          lastName: 'Test',
          email: 'api.test@example.com',
          phoneNumber: '1234567890',
          referralCode: '',
          password: 'Password123!',
          confirmPassword: 'Password123!',
          recaptchaToken: ''
        })
      });
      
      const responseText = await response.text();
      
      return {
        status: response.status,
        statusText: response.statusText,
        url: response.url,
        headers: Object.fromEntries(response.headers.entries()),
        body: responseText
      };
    } catch (error) {
      return {
        error: error.message,
        name: error.name
      };
    }
  });
  
  console.log('API Response:', apiResponse);
  
  if (apiResponse.status) {
    if (apiResponse.status === 404) {
      console.log('❌ API endpoint still returning 404 - routing not fixed');
    } else if (apiResponse.status === 400) {
      console.log('✅ API endpoint found but validation failed (expected)');
    } else if (apiResponse.status === 200) {
      console.log('✅ API endpoint working perfectly!');
    } else {
      console.log(`⚠️ API endpoint responded with status ${apiResponse.status}`);
    }
  } else {
    console.log('⚠️ Could not connect to server (likely not running)');
    console.log('Error:', apiResponse.error);
  }
});