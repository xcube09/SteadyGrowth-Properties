const { test, expect } = require('@playwright/test');

test('Test manual registration HTML file', async ({ page }) => {
  // Navigate to the manual test HTML file
  await page.goto('file:///C:/Users/ebimi/OneDrive/Desktop/SteadyGrowth/SteadyGrowth.Web/tests/registration-manual-test.html');
  
  // Check if the page loads
  await expect(page.locator('h2')).toContainText('Registration Form Test');
  
  // Check if form exists and is visible
  const form = page.locator('#registerForm');
  await expect(form).toBeVisible();
  
  // Test form validation
  await page.click('#submitBtn');
  await page.waitForTimeout(1000);
  
  // Check if jQuery validation is working
  const validationResult = await page.locator('#validationResult').textContent();
  console.log('Validation result:', validationResult);
  
  // Fill the form with test data
  await page.fill('#firstName', 'Test');
  await page.fill('#lastName', 'User');
  await page.fill('#email', 'test@example.com');
  await page.fill('#password', 'Password123!');
  await page.fill('#confirmPassword', 'Password123!');
  
  // Check consent checkboxes - force check even if they have validation errors
  await page.locator('#consent1').check({ force: true });
  await page.locator('#consent2').check({ force: true });
  
  // Submit the form
  await page.click('#submitBtn');
  await page.waitForTimeout(3000);
  
  // Check AJAX result
  const ajaxResult = await page.locator('#ajaxResult').textContent();
  console.log('AJAX result:', ajaxResult);
  
  // Take screenshot
  await page.screenshot({ path: 'tests/manual-test-result.png' });
  
  console.log('Manual test completed successfully');
});