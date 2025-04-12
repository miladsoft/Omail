window.setTheme = (isDarkMode) => {
    if (isDarkMode) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
};

window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text)
        .then(() => console.log('Text copied to clipboard'))
        .catch(err => console.error('Error copying text: ', err));
};

window.scrollToElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};

window.focusElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};

window.downloadFile = (filename, contentType, content) => {
    // Convert the content from base64 if provided as such
    const data = contentType.indexOf("base64") !== -1 ? 
        content : 
        contentType.indexOf("json") !== -1 ? 
            JSON.stringify(content) : 
            content;
    
    const file = new Blob([data], { type: contentType });
    const a = document.createElement("a");
    a.href = URL.createObjectURL(file);
    a.download = filename;
    a.click();
    URL.revokeObjectURL(a.href);
};

// Add event listener for drag-and-drop file uploads
window.initializeFileDropZone = (dropZoneElement, inputElement) => {
    const inputFile = document.getElementById(inputElement);
    
    if (!dropZoneElement || !inputFile) return;
    
    // Add highlight class when dragging over
    dropZoneElement.addEventListener('dragenter', () => {
        dropZoneElement.classList.add('border-omail-500');
    });
    
    dropZoneElement.addEventListener('dragleave', () => {
        dropZoneElement.classList.remove('border-omail-500');
    });
    
    dropZoneElement.addEventListener('dragover', (e) => {
        e.preventDefault();
    });
    
    // Handle the drop event
    dropZoneElement.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZoneElement.classList.remove('border-omail-500');
        
        if (e.dataTransfer.files.length) {
            inputFile.files = e.dataTransfer.files;
            const event = new Event('change', { bubbles: true });
            inputFile.dispatchEvent(event);
        }
    });
};
