document.addEventListener('DOMContentLoaded', () => {
    const inputs = document.querySelectorAll('input, select, textarea');

    inputs.forEach(input => {
        // 1. İlk yüklemede kontrol et
        if (input.classList.contains('input-validation-error')) {
            input.classList.add('is-invalid');
        }

        // 2. Sadece bu input'u izle
        const observer = new MutationObserver(mutations => {
            for (const mutation of mutations) {
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                    const hasError = input.classList.contains('input-validation-error');
                    const isInvalid = input.classList.contains('is-invalid');

                    if (hasError && !isInvalid) {
                        input.classList.add('is-invalid');
                    } else if (!hasError && isInvalid) {
                        input.classList.remove('is-invalid');
                    }
                }
            }
        });

        observer.observe(input, {
            attributes: true,
            attributeFilter: ['class']
        });
    });
}); 