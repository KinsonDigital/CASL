<!--
    !! NOTE !! - ONLY PROJECT OWNERS AND MAINTAINERS MANAGE PRODUCTION PREVIEW RELEASE PULL REQUESTS
    If you have contributions to make, use the "feature-to-develop" pull request template.
-->

<!-- Provide a short general summary of your changes in the Title above -->
## Production Release PR Description
This pull request performs a production release for version [add version here]

## How Has This Been Tested?
- [ ] Testing Application (Manual)

---

## Optional Checklist:
- [ ] Bug Fix (non-breaking change which fixes an issue)
- [ ] Breaking change (fix or feature that would cause existing functionality to change)
- [ ] My change requires a change to the documentation.
- [ ] I have added tests to cover my changes.
  - [ ] I have updated the documentation accordingly.
  - [ ] If changes to documentation have been made, the PR contains the **documentation** label.

---

## Required Checklist (All Must Be Reviewed And Checked):
<!-- Go over all the following points, and put an `x` in all the boxes that apply. -->
<!-- If you're unsure about any of these, don't hesitate to ask. We're here to help! -->
- [ ] PR title matches the example below but with proper version
  * Release To Production - v1.2.3
- [ ] The ***[add version here]*** text in the PR description replaced with the version.
- [ ] An issue exists and is linked to this PR.
- [ ] This PR is only for bringing changes from a ***version-release*** branches into a the ***master*** branch
    - 💡 A ***version-release*** branch is the branch used for preview releases and has a syntax of ***release/v1.2.3***
- [ ] My code follows the code style of this project.
- [ ] All tests passed locally.
  - 💡 Status checks are put in place to run unit tests every single time a change is pushed to a PR.  This does not mean that the tests pass in both the local and CI environment.
- [ ] Update library version by updating the ***\<Version/\>*** and ***\<FileVersion/\>*** tags in the ***.csproj*** file.
  - 💡 Every change to a PR will run a status check to confirm that the version has the correct syntax and does not already exist in the repository.
  - 💡 Make sure to remove the ***.preview.\<number\>*** section from the end of the version
    - **Example:**
      - The versions should be changed from:
        ``` html
        <Version>1.2.3.preview.4</Version>
        <FileVersion>1.2.3.preview.4</FileVersion>
        ```
      - To this:
        ``` html
        <Version>1.2.3</Version>
        <FileVersion>1.2.3</FileVersion>
        ```
