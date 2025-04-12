// File input click functionality
window.clickFileInputElement = (dotNetHelper) => {
    // Make the method available for .NET code to call
};

// Function to trigger a click on a file input by its ID
window.clickFileInput = (inputId) => {
    const element = document.getElementById(inputId);
    if (element) {
        element.click();
    } else {
        console.error(`File input with id '${inputId}' not found`);
    }
};