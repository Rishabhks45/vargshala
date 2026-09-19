/**
 * Vargshala Messaging & Chat Helper
 * Manages auto-growing textarea, keyboard shortcuts (Enter to send, Shift+Enter for new line), and scroll management.
 */
window.vargshalaChat = {
    initComposer: function (textareaId, sendButtonId) {
        var ta = document.getElementById(textareaId || 'chat-composer-textarea');
        if (!ta || ta._chatComposerBound) return;
        ta._chatComposerBound = true;

        ta.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                var btn = document.getElementById(sendButtonId || 'chat-send-btn');
                if (btn && !btn.disabled) {
                    btn.click();
                }
            } else if (e.key === 'Enter' && e.shiftKey) {
                setTimeout(function () {
                    window.vargshalaChat.autoResize(ta);
                }, 10);
            }
        });

        ta.addEventListener('input', function () {
            window.vargshalaChat.autoResize(ta);
        });
    },

    autoResize: function (elementOrId) {
        var ta = typeof elementOrId === 'string' ? document.getElementById(elementOrId) : elementOrId;
        if (!ta) return;
        ta.style.height = 'auto';
        ta.style.height = Math.min(ta.scrollHeight, 160) + 'px';
    },

    insertEmoji: function (textareaId, emoji) {
        var ta = document.getElementById(textareaId || 'chat-composer-textarea');
        if (!ta) return;
        window.vargshalaChat.autoResize(ta);
        ta.focus();
    },

    resetHeight: function (textareaId, height) {
        var ta = document.getElementById(textareaId || 'chat-composer-textarea');
        if (ta) {
            ta.style.height = (height || '24px');
        }
    },

    scrollToBottom: function (containerId, delay) {
        setTimeout(function () {
            var el = document.getElementById(containerId || 'messages-stream');
            if (el) {
                el.scrollTop = el.scrollHeight;
            }
        }, delay || 60);
    }
};
