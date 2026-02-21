import { AbsoluteFill, Series } from "remotion";
import { TitleScene } from "./scenes/TitleScene";
import { ProblemScene } from "./scenes/ProblemScene";
import { ArchitectureScene } from "./scenes/ArchitectureScene";
import { DealFlowScene } from "./scenes/DealFlowScene";
import { ExposureLookupScene } from "./scenes/ExposureLookupScene";
import { TechStackScene } from "./scenes/TechStackScene";
import { ResumeScene } from "./scenes/ResumeScene";
import { ClosingScene } from "./scenes/ClosingScene";

export const DealFlowDemo: React.FC = () => {
  return (
    <AbsoluteFill>
      <Series>
        {/* Scene 1: Personal hook */}
        <Series.Sequence durationInFrames={180}>
          <TitleScene />
        </Series.Sequence>

        {/* Scene 2: The daily reality — Vision + SSRS pain */}
        <Series.Sequence durationInFrames={360}>
          <ProblemScene />
        </Series.Sequence>

        {/* Scene 3: "2 nights. After work." transition — extended for readability */}
        <Series.Sequence durationInFrames={540}>
          <DealFlowScene />
        </Series.Sequence>

        {/* Scene 4: What I built — architecture diagram (19s to absorb) */}
        <Series.Sequence durationInFrames={570}>
          <ArchitectureScene />
        </Series.Sequence>

        {/* Scene 5: The exposure lookup solution */}
        <Series.Sequence durationInFrames={480}>
          <ExposureLookupScene />
        </Series.Sequence>

        {/* Scene 6: GitHub + parallel tracks (22s — needs time to absorb) */}
        <Series.Sequence durationInFrames={660}>
          <TechStackScene />
        </Series.Sequence>

        {/* Scene 7: Resume flash */}
        <Series.Sequence durationInFrames={270}>
          <ResumeScene />
        </Series.Sequence>

        {/* Scene 8: Closing — "This is just one idea" */}
        <Series.Sequence durationInFrames={390}>
          <ClosingScene />
        </Series.Sequence>
      </Series>
    </AbsoluteFill>
  );
};
