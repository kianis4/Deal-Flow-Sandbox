import { AbsoluteFill, interpolate, useCurrentFrame, spring, useVideoConfig } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 3: The "spark" transition — humble, honest, personal.
 * Slower pacing so every line is readable. 450 frames = 15 seconds.
 */
export const DealFlowScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  // Line 1: Current role context (frames 10-35)
  const roleOpacity = interpolate(frame, [10, 30], [0, 1], { extrapolateRight: "clamp" });
  const roleY = spring({ frame: frame - 10, fps, from: 18, to: 0, durationInFrames: 22 });

  // Line 2: "I've been thinking..." (frames 50-75)
  const line1Opacity = interpolate(frame, [50, 70], [0, 1], { extrapolateRight: "clamp" });

  // Line 3: "2 nights. After work." (frames 95-120)
  const line2Opacity = interpolate(frame, [95, 115], [0, 1], { extrapolateRight: "clamp" });
  const line2Y = spring({ frame: frame - 95, fps, from: 20, to: 0, durationInFrames: 25 });

  // Line 4: PostgreSQL mock description (frames 150-175)
  const line3Opacity = interpolate(frame, [150, 175], [0, 1], { extrapolateRight: "clamp" });

  // Divider (frames 195-225)
  const dividerWidth = spring({ frame: frame - 195, fps, from: 0, to: 200, durationInFrames: 30 });

  // Line 5: "This isn't Vision..." (frames 230-260)
  const line4Opacity = interpolate(frame, [230, 255], [0, 1], { extrapolateRight: "clamp" });

  // Line 6: "I know the real systems..." (frames 280-310)
  const line5Opacity = interpolate(frame, [280, 305], [0, 1], { extrapolateRight: "clamp" });

  // Line 7: "I'd love to learn..." (frames 330-360) — earlier start, more hold time
  const line6Opacity = interpolate(frame, [330, 355], [0, 1], { extrapolateRight: "clamp" });
  const line6Y = spring({ frame: frame - 330, fps, from: 12, to: 0, durationInFrames: 22 });

  return (
    <AbsoluteFill
      style={{
        background: `linear-gradient(160deg, #1B1B2F 0%, #1A2332 50%, #162230 100%)`,
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: fonts.heading,
      }}
    >
      <div style={{ textAlign: "center", maxWidth: 920 }}>
        {/* Current role context */}
        <p
          style={{
            fontSize: 18,
            fontWeight: 400,
            color: "#556677",
            margin: "0 0 28px 0",
            opacity: roleOpacity,
            transform: `translateY(${roleY}px)`,
            lineHeight: 1.5,
          }}
        >
          As a Sales Analyst under Siraaj, Director of Sales,
          <br />
          I structure deals, run exposure checks, and see the friction every day.
        </p>

        {/* Thinking line */}
        <p
          style={{
            fontSize: 24,
            fontWeight: 400,
            color: "#8899AA",
            margin: "0 0 18px 0",
            opacity: line1Opacity,
          }}
        >
          I&apos;ve been thinking about how to solve problems like this for a long time.
        </p>

        {/* Hero line */}
        <h1
          style={{
            fontSize: 60,
            fontWeight: 700,
            color: "#FFFFFF",
            margin: "0 0 18px 0",
            opacity: line2Opacity,
            transform: `translateY(${line2Y}px)`,
            lineHeight: 1.2,
          }}
        >
          2 nights. After work.
        </h1>

        {/* What I did */}
        <p
          style={{
            fontSize: 22,
            fontWeight: 400,
            color: "#667788",
            margin: "0 0 36px 0",
            opacity: line3Opacity,
            lineHeight: 1.6,
          }}
        >
          I spun up a PostgreSQL instance, modeled the deal structure from what I know,
          <br />
          and built a simple working mockup.
        </p>

        {/* Divider */}
        <div
          style={{
            width: dividerWidth,
            height: 2,
            backgroundColor: colors.primary,
            margin: "0 auto 36px auto",
            borderRadius: 1,
          }}
        />

        {/* Honest framing */}
        <p
          style={{
            fontSize: 20,
            fontWeight: 400,
            color: "#556677",
            margin: "0 0 18px 0",
            opacity: line4Opacity,
            lineHeight: 1.6,
          }}
        >
          This isn&apos;t Vision — it&apos;s a replica of the deal pipeline concepts
          <br />
          paired with an automated version of the SSRS exposure lookup.
        </p>

        <p
          style={{
            fontSize: 20,
            fontWeight: 400,
            color: "#778899",
            margin: "0 0 28px 0",
            opacity: line5Opacity,
            lineHeight: 1.6,
          }}
        >
          The real systems — Vision, Location, the internal platforms — are far more complex.
        </p>

        {/* Forward-looking */}
        <p
          style={{
            fontSize: 22,
            fontWeight: 500,
            color: "#AAB8C8",
            margin: 0,
            opacity: line6Opacity,
            transform: `translateY(${line6Y}px)`,
            lineHeight: 1.6,
          }}
        >
          I&apos;d love the chance to learn the full scope — and grow into it.
        </p>
      </div>
    </AbsoluteFill>
  );
};
