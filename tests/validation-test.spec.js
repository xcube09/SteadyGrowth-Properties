const { test, expect } = require('@playwright/test');

test.describe('Registration Form Validation Tests', () => {
  test('Should prevent submission with invalid inputs and show validation messages', async ({ page }) => {
    // Navigate to the manual test HTML file
    await page.goto('file:///C:/Users/ebimi/OneDrive/Desktop/SteadyGrowth/SteadyGrowth.Web/tests/registration-manual-test.html');
    
    console.log('Testing form validation behavior...');
    
    // Wait for form to load
    await page.waitForSelector('#registerForm');
    
    // 1. Test empty form submission
    console.log('1. Testing empty form submission...');
    await page.click('#submitBtn');
    await page.waitForTimeout(1000);
    
    // Check that validation errors appear
    const firstNameError = await page.locator('.field-validation[data-field="firstName"]').textContent();
    const lastNameError = await page.locator('.field-validation[data-field="lastName"]').textContent();
    const emailError = await page.locator('.field-validation[data-field="email"]').textContent();
    const passwordError = await page.locator('.field-validation[data-field="password"]').textContent();
    const consent1Error = await page.locator('.field-validation[data-field="consent1"]').textContent();
    const consent2Error = await page.locator('.field-validation[data-field="consent2"]').textContent();
    
    console.log('Validation errors found:');
    console.log('- First Name:', firstNameError);
    console.log('- Last Name:', lastNameError);
    console.log('- Email:', emailError);
    console.log('- Password:', passwordError);
    console.log('- Consent1:', consent1Error);
    console.log('- Consent2:', consent2Error);
    
    // Verify that errors are displayed
    expect(firstNameError).toContain('First name is required');
    expect(lastNameError).toContain('Last name is required');
    expect(emailError).toContain('Email is required');
    expect(passwordError).toContain('Password is required');
    expect(consent1Error).toContain('You must consent to data processing');
    expect(consent2Error).toContain('You must accept the privacy policy');
    
    // 2. Test invalid email format
    console.log('2. Testing invalid email format...');
    await page.fill('#firstName', 'Test');
    await page.fill('#lastName', 'User');
    await page.fill('#email', 'invalid-email');
    await page.click('#submitBtn');
    await page.waitForTimeout(1000);
    
    const emailFormatError = await page.locator('.field-validation[data-field="email"]').textContent();
    console.log('Email format error:', emailFormatError);
    expect(emailFormatError).toContain('valid email');
    
    // 3. Test password mismatch
    console.log('3. Testing password mismatch...');
    await page.fill('#email', 'test@example.com');
    await page.fill('#password', 'password123');
    await page.fill('#confirmPassword', 'differentpassword');
    await page.click('#submitBtn');
    await page.waitForTimeout(1000);
    
    const passwordMismatchError = await page.locator('.field-validation[data-field="confirmPassword"]').textContent();
    console.log('Password mismatch error:', passwordMismatchError);
    expect(passwordMismatchError).toContain('match');
    
    // 4. Test short password
    console.log('4. Testing short password...');
    await page.fill('#password', '12345');
    await page.fill('#confirmPassword', '12345');
    await page.click('#submitBtn');
    await page.waitForTimeout(1000);
    
    const shortPasswordError = await page.locator('.field-validation[data-field="password"]').textContent();
    console.log('Short password error:', shortPasswordError);
    expect(shortPasswordError).toContain('at least 6');
    
    // 5. Test form with valid data but missing consents
    console.log('5. Testing valid data but missing consents...');
    await page.fill('#password', 'Password123!');
    await page.fill('#confirmPassword', 'Password123!');
    await page.click('#submitBtn');
    await page.waitForTimeout(1000);
    
    // Should still show consent errors
    const finalConsent1Error = await page.locator('.field-validation[data-field="consent1"]').textContent();
    const finalConsent2Error = await page.locator('.field-validation[data-field="consent2"]').textContent();
    console.log('Final consent1 error:', finalConsent1Error);
    console.log('Final consent2 error:', finalConsent2Error);
    expect(finalConsent1Error).toContain('You must consent to data processing');
    expect(finalConsent2Error).toContain('You must accept the privacy policy');
    
    // 6. Test form with all valid data
    console.log('6. Testing form with all valid data...');
    await page.check('#consent1');
    await page.check('#consent2');
    await page.click('#submitBtn');
    await page.waitForTimeout(2000);
    
    // Should attempt AJAX call (will fail since no server, but form validation passed)
    const ajaxResult = await page.locator('#ajaxResult').textContent();
    console.log('AJAX result with valid form:', ajaxResult);
    expect(ajaxResult).toContain('AJAX call failed'); // Expected since no server running
    
    // 7. Verify no validation errors remain
    const finalFirstNameError = await page.locator('.field-validation[data-field="firstName"]').textContent();
    const finalLastNameError = await page.locator('.field-validation[data-field="lastName"]').textContent();
    const finalEmailError = await page.locator('.field-validation[data-field="email"]').textContent();
    const finalPasswordError = await page.locator('.field-validation[data-field="password"]').textContent();
    
    console.log('Final validation state:');
    console.log('- First Name errors:', finalFirstNameError);
    console.log('- Last Name errors:', finalLastNameError);
    console.log('- Email errors:', finalEmailError);
    console.log('- Password errors:', finalPasswordError);
    
    // These should be empty since validation passed
    expect(finalFirstNameError.trim()).toBe('');
    expect(finalLastNameError.trim()).toBe('');
    expect(finalEmailError.trim()).toBe('');
    expect(finalPasswordError.trim()).toBe('');
    
    // Take screenshot of final state
    await page.screenshot({ path: 'tests/validation-test-final.png' });
    
    console.log('✅ All validation tests passed!');
  });
});