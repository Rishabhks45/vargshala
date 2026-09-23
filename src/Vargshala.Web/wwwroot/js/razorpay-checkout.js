// Interop helper to invoke official Razorpay Checkout SDK
window.RazorpayInterop = {
    openCheckout: function (options, dotNetHelper) {
        if (typeof Razorpay === 'undefined') {
            console.error('Razorpay SDK script not loaded');
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnRazorpayFailed', 'Razorpay checkout SDK failed to load. Please check your internet connection.');
            }
            return;
        }

        try {
            var rzpOptions = {
                key: options.keyId,
                amount: Math.round(options.amount * 100), // convert to paise
                currency: options.currency || 'INR',
                name: options.companyName || 'Vargshala SaaS',
                description: options.description || 'SaaS Subscription Plan',
                order_id: options.orderId,
                theme: {
                    color: options.themeColor || '#004D40'
                },
                prefill: {
                    name: options.prefillName || '',
                    email: options.prefillEmail || '',
                    contact: options.prefillContact || ''
                },
                notes: options.notes || {},
                handler: function (response) {
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync(
                            'OnRazorpaySuccess',
                            response.razorpay_order_id,
                            response.razorpay_payment_id,
                            response.razorpay_signature
                        );
                    }
                },
                modal: {
                    ondismiss: function () {
                        if (dotNetHelper) {
                            dotNetHelper.invokeMethodAsync('OnRazorpayDismissed');
                        }
                    }
                }
            };

            var rzp = new Razorpay(rzpOptions);
            rzp.on('payment.failed', function (response) {
                var reason = (response && response.error && response.error.description) 
                    ? response.error.description 
                    : 'Payment was not completed.';
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('OnRazorpayFailed', reason);
                }
            });

            rzp.open();
        } catch (err) {
            console.error('Error opening Razorpay checkout:', err);
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnRazorpayFailed', err.message || 'Error launching payment dialog');
            }
        }
    }
};
