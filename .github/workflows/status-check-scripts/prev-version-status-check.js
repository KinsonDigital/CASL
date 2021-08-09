module.exports = ({version}) => {
    let correctSyntaxMsg = "Version Syntax --> <major>.<minor>.<patch>-preview.<preview-version>";
    let exampleMsg = "Example: 1.2.3-preview.4";

    let hyphenResult = validateHyphen(version);
    
    if (hyphenResult.failed) {
        return hyphenResult;
    }

    // The sections to the left and right of the hyphen
    let mainSections = version.split("-");
    let majorMinorPatchSection = mainSections[0];

    let majorMinorPatchResult = validateMajorMinorPatch(majorMinorPatchSection);

    if (majorMinorPatchResult.failed) {
        return majorMinorPatchResult;
    }

    let previewSection = mainSections[1];

    let previewSectionResult = validatePreview(previewSection);

    if (previewSectionResult.failed) {
        return previewSectionResult;
    }

    /**
     * Returns an object describing if the version string contains a hyphen with an error message.
     * @param {string} version The version string to validate against.
     */
    function validateHyphen(version) {
        // If a hyphen '-' does not exist
        if (!version.includes("-")) {
            let errorMsg = `
            The 'major|minor|patch' section and the 'preview' section must be separated by a hyphen.
                ${correctSyntaxMsg}

                ${exampleMsg}
                'major-minor-patch'      'preview'
                        |_____         ______| 
                              |       |
                            |---| |-------|
                            1.2.3-preview.4
            `;

            return { "failed": true, "error": errorMsg };
        }

        // If there are too many hyphens
        if (version.split("-").length > 2) {
            let errorMsg = `
            Only 1 hyphen is aloud.
                ${correctSyntaxMsg}
                ${exampleMsg}
            `;

            return { "failed": true, "error": errorMsg };
        }

        // If the version starts or ends with a hyphen
        if (version.startsWith("-") || version.endsWith("-")) {
            let errorMsg = `
            The version must not start or end with a '-' (hyphen) character.
                ${correctSyntaxMsg}
                ${exampleMsg}
            `;

            return { "failed": true, "error": errorMsg };
        }

        return { "failed": false, "error": "no error" };
    }

    /**
     * Validates the major, minor, and patch numbers of the given section
     * @param {string} majorMinorPatch The major minor and patch section of a preview version.
     */
    function validateMajorMinorPatch(majorMinorPatch) {
        let sections = majorMinorPatch.split(".");

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

    /**
     * Validates that the given preview string is valid.
     * @param {string} preview The preview section of a version number
     */
    function validatePreview(preview) {
        let previewSections = preview.split(".");

        // If the preview does not contain a period symbol
        if (!preview.includes(".") && previewSections.length != 2) {
            let errorMsg = `
            The preview section should contain a single period symbol.
                ${correctSyntaxMsg}
                ${exampleMsg}
            `;

            return { "failed": true, "error": errorMsg };
        }

        // If the preview string must be exact
        if (previewSections[0] !== 'preview') {
            let errorMsg = `
            The preview section must start with the value of 'preview'
                ${correctSyntaxMsg}
                ${exampleMsg}
            `;
            
            return { "failed": true, "error": errorMsg };
        }

        // If the preview number not a number
        if (isNaN(previewSections[1])) {
            let errorMsg = `
            The preview number of '${previewSections[1]}' must be a whole number.
                ${correctSyntaxMsg}
                ${exampleMsg}
            `;
            
            return { "failed": true, "error": errorMsg };
        }

        return { "failed": false, "error": "no error" };
    }

    return { "failed": false, "error": "no error" };
}
