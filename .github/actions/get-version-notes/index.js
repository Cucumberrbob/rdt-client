const core = require("@actions/core");
const fs = require("fs");
const { parser } = require("keep-a-changelog");

(async () => {
  try {
    const version = core.getInput("version");
    const changelog = parser(fs.readFileSync("CHANGELOG.md", "utf8"));

    const release = changelog.findRelease(version);
    if (!release) {
      throw new Error(`Could not find section for version ${version}`);
    }

    const notes = release.toString().split("\n").slice(1).join("\n");
    core.setOutput("notes", notes);
  } catch (error) {
    core.setFailed(error.message);
  }
}
)();
