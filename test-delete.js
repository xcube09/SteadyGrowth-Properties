const { chromium } = require('playwright');

(async () => {
    const browser = await chromium.launch({ 
        headless: false,
        devtools: true 
    });
    const context = await browser.newContext();
    const page = await context.newPage();

    // Enable console logging
    page.on('console', msg => console.log('Browser console:', msg.text()));
    page.on('pageerror', error => console.log('Page error:', error.message));
    
    // Monitor network requests
    page.on('request', request => {
        if (request.url().includes('Delete')) {
            console.log('Delete request:', {
                url: request.url(),
                method: request.method(),
                headers: request.headers(),
                postData: request.postData()
            });
        }
    });

    page.on('response', response => {
        if (response.url().includes('Delete')) {
            console.log('Delete response:', {
                url: response.url(),
                status: response.status(),
                statusText: response.statusText()
            });
        }
    });

    try {
        console.log('Navigating to application...');
        await page.goto('https://localhost:7147', { waitUntil: 'networkidle' });
        
        // Check if we need to login
        if (page.url().includes('Login') || await page.locator('a:has-text("Login")').isVisible()) {
            console.log('Need to login first...');
            
            // Navigate to login if not already there
            if (!page.url().includes('Login')) {
                await page.click('a:has-text("Login")');
            }
            
            // Perform login
            await page.fill('input[name="Input.Email"]', 'admin@steadygrowth.com');
            await page.fill('input[name="Input.Password"]', 'Admin@123');
            await page.click('button[type="submit"]:has-text("Log in")');
            
            await page.waitForNavigation();
            console.log('Logged in successfully');
        }

        // Navigate to admin properties page
        console.log('Navigating to admin properties page...');
        await page.goto('https://localhost:7147/Admin/Properties/Index', { waitUntil: 'networkidle' });
        
        // Wait for the table to load
        await page.waitForSelector('#kt_table_properties', { timeout: 10000 });
        console.log('Properties table loaded');

        // Check if there are any properties
        const rows = await page.locator('#kt_table_properties tbody tr').count();
        console.log(`Found ${rows} property rows`);

        if (rows > 0) {
            // Check for the delete button in the first row
            const firstRow = page.locator('#kt_table_properties tbody tr').first();
            
            // Click the Actions dropdown
            console.log('Clicking Actions dropdown...');
            await firstRow.locator('a:has-text("Actions")').click();
            
            // Wait for dropdown menu
            await page.waitForSelector('.menu-sub-dropdown', { state: 'visible' });
            
            // Check if delete option exists
            const deleteButton = page.locator('[data-kt-property-table-filter="delete_row"]').first();
            const deleteExists = await deleteButton.isVisible();
            console.log('Delete button visible:', deleteExists);
            
            if (deleteExists) {
                // Click delete
                console.log('Clicking delete button...');
                await deleteButton.click();
                
                // Wait for SweetAlert
                await page.waitForSelector('.swal2-container', { state: 'visible', timeout: 5000 });
                console.log('SweetAlert appeared');
                
                // Click confirm
                await page.click('.swal2-confirm');
                console.log('Clicked confirm on SweetAlert');
                
                // Wait for response
                await page.waitForTimeout(3000);
                
                // Check if any error messages appear
                const errorVisible = await page.locator('.swal2-error').isVisible();
                if (errorVisible) {
                    const errorText = await page.locator('.swal2-html-container').textContent();
                    console.log('Error message:', errorText);
                }
            }
        } else {
            console.log('No properties found in the table');
        }

        // Keep browser open for inspection
        console.log('Test complete. Browser will remain open for inspection...');
        await page.waitForTimeout(60000);

    } catch (error) {
        console.error('Error during test:', error);
        await page.screenshot({ path: 'error-screenshot.png' });
    } finally {
        await browser.close();
    }
})();