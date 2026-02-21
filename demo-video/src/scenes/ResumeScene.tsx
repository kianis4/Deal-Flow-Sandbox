import { AbsoluteFill, Img, interpolate, useCurrentFrame, spring, useVideoConfig, staticFile } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 7: Resume flash — all 3 pages scroll through.
 * 270 frames (9s). Each page gets ~3 seconds of visibility.
 *
 * Timeline:
 *   0-25:    Label fades in
 *   15-35:   Resume container fades in + springs
 *   35-100:  Page 1 holds
 *   100-130: Scroll to page 2
 *   130-190: Page 2 holds
 *   190-220: Scroll to page 3
 *   220-270: Page 3 holds
 */
export const ResumeScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const labelOpacity = interpolate(frame, [5, 20], [0, 1], { extrapolateRight: "clamp" });
  const resumeOpacity = interpolate(frame, [15, 35], [0, 1], { extrapolateRight: "clamp" });
  const resumeScale = spring({ frame: frame - 15, fps, from: 0.92, to: 1, durationInFrames: 25 });

  // Scroll through 3 pages — each page ~802px tall at 620px width
  // Page 1: hold (35-100), scroll to page 2 (100-130), hold (130-190),
  // scroll to page 3 (190-220), hold (220-270)
  const scrollY =
    frame < 100
      ? 0
      : frame < 130
        ? interpolate(frame, [100, 130], [0, -802], { extrapolateRight: "clamp" })
        : frame < 190
          ? -802
          : frame < 220
            ? interpolate(frame, [190, 220], [-802, -1604], { extrapolateRight: "clamp" })
            : -1604;

  const linkOpacity = interpolate(frame, [80, 100], [0, 1], { extrapolateRight: "clamp" });

  // Page indicator dots
  const currentPage = frame < 115 ? 0 : frame < 205 ? 1 : 2;

  return (
    <AbsoluteFill
      style={{
        background: `linear-gradient(160deg, #FAFBFD 0%, #EEF2F7 50%, #E3EAF3 100%)`,
        fontFamily: fonts.heading,
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        padding: "24px 60px",
      }}
    >
      {/* Label */}
      <div
        style={{
          opacity: labelOpacity,
          textAlign: "center",
          marginBottom: 12,
        }}
      >
        <p
          style={{
            fontSize: 16,
            fontWeight: 600,
            color: colors.primary,
            letterSpacing: 2,
            textTransform: "uppercase",
            margin: "0 0 4px 0",
          }}
        >
          Full Resume
        </p>
        <p style={{ fontSize: 14, color: colors.textMuted, margin: 0 }}>
          suleyman.io
        </p>
      </div>

      {/* Resume pages with scroll */}
      <div
        style={{
          opacity: resumeOpacity,
          transform: `scale(${resumeScale})`,
          borderRadius: 12,
          overflow: "hidden",
          border: `1.5px solid ${colors.border}`,
          boxShadow: "0 8px 40px rgba(0,0,0,0.12)",
          maxHeight: 820,
          width: 620,
        }}
      >
        <div style={{ transform: `translateY(${scrollY}px)` }}>
          <Img
            src={staticFile("resume-page-1.png")}
            style={{ width: 620, display: "block" }}
          />
          <Img
            src={staticFile("resume-page-2.png")}
            style={{ width: 620, display: "block" }}
          />
          <Img
            src={staticFile("resume-page-3.png")}
            style={{ width: 620, display: "block" }}
          />
        </div>
      </div>

      {/* Page indicator dots */}
      <div
        style={{
          display: "flex",
          gap: 8,
          marginTop: 12,
          opacity: resumeOpacity,
        }}
      >
        {[0, 1, 2].map((i) => (
          <div
            key={i}
            style={{
              width: currentPage === i ? 20 : 8,
              height: 8,
              borderRadius: 4,
              backgroundColor: currentPage === i ? colors.primary : `${colors.primary}40`,
              transition: "all 0.3s",
            }}
          />
        ))}
      </div>

      {/* Subtle links */}
      <div
        style={{
          opacity: linkOpacity,
          marginTop: 10,
          display: "flex",
          gap: 24,
          fontSize: 14,
          color: colors.textMuted,
        }}
      >
        <span>github.com/kianis4</span>
        <span>linkedin.com/in/suleyman-kiani</span>
        <span>suleyman.io</span>
      </div>
    </AbsoluteFill>
  );
};
