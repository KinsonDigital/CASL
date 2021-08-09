module.exports = ({version}) => {
    let correctSyntaxMsg = "Version Syntax --> <major>.<minor>.<patch>";
    let exampleMsg = "Example: 1.2.3";
    let sections = version.split(".");

    // If there are not exactly 3 sections
    if (sections.length != 3) {
        let errorMsg = `
        The version must have a major, minor, and patch number.
            ${correctSyntaxMsg}
            ${exampleMsg}
        `;

        return { "failed": true, "error": errorMsg };            
    }

    // If the version major is not a number
    if (isNaN(sections[0])) {
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

    return { "failed": false, "error": "no error" };
}
