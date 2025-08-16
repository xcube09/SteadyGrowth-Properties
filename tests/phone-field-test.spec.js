const { test, expect } = require('@playwright/test');

test('Check phone number field appearance', async ({ page }) => {
  // Navigate to the manual test HTML file
  await page.goto('file:///C:/Users/ebimi/OneDrive/Desktop/SteadyGrowth/SteadyGrowth.Web/tests/registration-manual-test.html');
  
  // Wait for form to load
  await page.waitForSelector('#registerForm');
  
  // Get styling information for different fields
  const firstName = page.locator('#firstName');
  const lastName = page.locator('#lastName');
  const email = page.locator('#email');
  const phoneNumber = page.locator('#phoneNumber');
  
  // Check if fields are visible
  await expect(firstName).toBeVisible();
  await expect(lastName).toBeVisible();
  await expect(email).toBeVisible();
  await expect(phoneNumber).toBeVisible();
  
  // Get computed styles
  const firstNameStyles = await firstName.evaluate(el => {
    const styles = window.getComputedStyle(el);
    return {
      display: styles.display,
      border: styles.border,
      backgroundColor: styles.backgroundColor,
      width: styles.width,
      height: styles.height,
      padding: styles.padding,
      margin: styles.margin
    };
  });
  
  const phoneStyles = await phoneNumber.evaluate(el => {
    const styles = window.getComputedStyle(el);
    return {
      display: styles.display,
      border: styles.border,
      backgroundColor: styles.backgroundColor,
      width: styles.width,
      height: styles.height,
      padding: styles.padding,
      margin: styles.margin
    };
  });
  
  console.log('First Name field styles:', firstNameStyles);
  console.log('Phone Number field styles:', phoneStyles);
  
  // Check attributes
  const firstNameAttrs = await firstName.evaluate(el => ({
    required: el.hasAttribute('required'),
    class: el.className,
    type: el.type,
    placeholder: el.placeholder
  }));
  
  const phoneAttrs = await phoneNumber.evaluate(el => ({
    required: el.hasAttribute('required'),
    class: el.className,
    type: el.type,
    placeholder: el.placeholder
  }));
  
  console.log('First Name attributes:', firstNameAttrs);
  console.log('Phone Number attributes:', phoneAttrs);
  
  // Take screenshot to see visual difference
  await page.screenshot({ path: 'tests/phone-field-comparison.png' });
  
  console.log('Phone field test completed');
});