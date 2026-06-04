globalThis.blazorHelpers = {
    focusAndSelect: (element) => {
        if (element && typeof element.focus === 'function') {
            element.focus();
            if (typeof element.select === 'function') element.select();
        }
    }
};
