// Global Site Scripts

// Add to Cart Analytics
function addToCart(productName) {
    // This function should be called by your Add to Cart buttons
    // Currently buttons are links or forms, we need to attach this to them

    // Log Analytics Event
    if (typeof logEvent === 'function') {
        logEvent('AddToCart', productName);
    }
}

// Attach listeners to all 'Add to Cart' buttons if they have a specific class
document.addEventListener("DOMContentLoaded", function () {
    // 1. Listen for buttons with class 'add-to-cart-btn'
    var buttons = document.querySelectorAll('.add-to-cart-btn');
    buttons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            var productName = this.getAttribute('data-product-name');
            if (productName) {
                addToCart(productName);
            }
        });
    });

    // 2. Listen for Details Page Form Submission
    var cartForm = document.getElementById('addToCartForm');
    if (cartForm) {
        cartForm.addEventListener('submit', function () {
            var productName = this.getAttribute('data-product-name');
            if (productName) {
                addToCart(productName);
            }
        });
    }
});
