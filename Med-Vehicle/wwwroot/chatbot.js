window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("userInput");

    if (input) {
        input.addEventListener("keydown", function (event) {
            if (event.key === "Enter") {
                event.preventDefault();

                // Busca el botón de enviar y simula el click
                const sendButton = document.getElementById("sendButton");
                if (sendButton) {
                    sendButton.click();
                }
            }
        });
    }
});
