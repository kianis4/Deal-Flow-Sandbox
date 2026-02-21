import { Composition } from "remotion";
import { DealFlowDemo } from "./Video";

export const Root: React.FC = () => {
  return (
    <Composition
      id="DealFlowDemo"
      component={DealFlowDemo}
      durationInFrames={3450}
      defaultProps={{}}
      calculateMetadata={() => ({ durationInFrames: 3450, fps: 30 })}
      fps={30}
      width={1920}
      height={1080}
    />
  );
};
