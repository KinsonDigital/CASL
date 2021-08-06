module.exports = ({github, context, core, version}) => {
    let sections = version.split('.');
    let correctSyntaxMsg = "Use the syntax --> <major>.<minor>.<patch>.preview.<preview-version>";
    let exampleMsg = "Example: 1.2.3.preview.4";
    
    // If the total sections are not equal to 5
    if (sections.length != 5) {
        let errorMsg = `
        There must be 5 parts to a preview version.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;

        return { "failed": true, "error": errorMsg };
    }

    // If the version major is not a number
    if (isNaN(new Number(sections[0]))) {
        let errorMsg = `
        There major value of '${sections[0]}' must be a whole number.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;

        return { "failed": true, "error": errorMsg };
    }

    // If the version minor is not a number
    if (isNaN(sections[1])) {
        let errorMsg = `
        There minor value of '${sections[1]}' must be a whole number.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;
        
        return { "failed": true, "error": errorMsg };
    }

    // If the version patch is not a number
    if (isNaN(sections[2])) {
        let errorMsg = `
        There patch value of '${sections[2]}' must be a whole number.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;
        
        return { "failed": true, "error": errorMsg };
    }

    // If the preview string must be exact
    if (sections[3] !== 'preview') {
        let errorMsg = `
        The preview section value of '${sections[3]}' is incorrect.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;
        
        return { "failed": true, "error": errorMsg };
    }

    // If the preview number not a number
    if (isNaN(sections[4])) {
        let errorMsg = `
        The preview number of '${sections[4]}' must be a whole number.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;
        
        return { "failed": true, "error": errorMsg };
    }

    return { "failed": false, "error": "no error" };
}
