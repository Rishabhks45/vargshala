// ==============================================================================
// Vargshala UI - CustomSelect Dropdown Click-Outside & Scroll Unblocker
// ==============================================================================
(function () {
    window.vargshalaSelect = {
        activeInstances: new Map(),
        initialized: false,

        register: function (id, dotNetRef) {
            this.activeInstances.set(id, dotNetRef);
            if (!this.initialized) {
                this.initialized = true;

                // 1. Click outside closes dropdown
                document.addEventListener('pointerdown', function (e) {
                    window.vargshalaSelect.activeInstances.forEach(function (ref, elId) {
                        var el = document.getElementById(elId);
                        if (el && !el.contains(e.target)) {
                            ref.invokeMethodAsync('CloseFromJs');
                        }
                    });
                }, true);

                // 2. Wheel event anywhere outside the menu closes dropdown and allows instant page scroll
                document.addEventListener('wheel', function (e) {
                    window.vargshalaSelect.activeInstances.forEach(function (ref, elId) {
                        var el = document.getElementById(elId);
                        if (el) {
                            var menu = el.querySelector('.custom-select-menu');
                            if (menu && (menu === e.target || menu.contains(e.target))) {
                                return; // Allow scrolling inside the options list
                            }
                            ref.invokeMethodAsync('CloseFromJs');
                        }
                    });
                }, { passive: true, capture: true });

                // 3. Scroll event on container or window closes dropdown
                window.addEventListener('scroll', function (e) {
                    window.vargshalaSelect.activeInstances.forEach(function (ref, elId) {
                        var el = document.getElementById(elId);
                        if (el) {
                            var menu = el.querySelector('.custom-select-menu');
                            if (menu && (menu === e.target || menu.contains(e.target))) {
                                return; // Allow scrolling inside the options list
                            }
                            ref.invokeMethodAsync('CloseFromJs');
                        }
                    });
                }, true);

                // 4. Touch move (mobile scroll) closes dropdown
                document.addEventListener('touchmove', function (e) {
                    window.vargshalaSelect.activeInstances.forEach(function (ref, elId) {
                        var el = document.getElementById(elId);
                        if (el) {
                            var menu = el.querySelector('.custom-select-menu');
                            if (menu && (menu === e.target || menu.contains(e.target))) {
                                return; // Allow touch scrolling inside the options list
                            }
                            ref.invokeMethodAsync('CloseFromJs');
                        }
                    });
                }, { passive: true, capture: true });

                // 5. Escape key closes dropdown
                document.addEventListener('keydown', function (e) {
                    if (e.key === 'Escape') {
                        window.vargshalaSelect.activeInstances.forEach(function (ref) {
                            ref.invokeMethodAsync('CloseFromJs');
                        });
                    }
                });
            }
        },

        unregister: function (id) {
            this.activeInstances.delete(id);
        }
    };
})();
