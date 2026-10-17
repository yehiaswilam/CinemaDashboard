// Confirm before submitting any form with data-confirm (used by delete buttons)
document.addEventListener('submit', function (e) {
    const msg = e.target.getAttribute('data-confirm');
    if (msg && !confirm(msg)) e.preventDefault();
});

// Live preview for a single image input: <input type="file" data-preview="#imgId">
document.addEventListener('change', function (e) {
    const input = e.target;
    if (input.type !== 'file') return;

    const single = input.getAttribute('data-preview');
    if (single && input.files && input.files[0]) {
        const img = document.querySelector(single);
        if (img) {
            img.src = URL.createObjectURL(input.files[0]);
            img.classList.remove('d-none');
        }
    }

    // Multiple previews: <input type="file" multiple data-preview-list="#containerId">
    const list = input.getAttribute('data-preview-list');
    if (list) {
        const box = document.querySelector(list);
        if (!box) return;
        box.innerHTML = '';
        Array.from(input.files || []).forEach(function (f) {
            const img = document.createElement('img');
            img.src = URL.createObjectURL(f);
            img.className = 'thumb-lg';
            box.appendChild(img);
        });
    }
});
