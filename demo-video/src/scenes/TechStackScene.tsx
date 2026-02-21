import { AbsoluteFill, Img, interpolate, useCurrentFrame, spring, useVideoConfig, staticFile } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 6: "There's more to me" — GitHub profile + personal story.
 * 660 frames (22s). Very generous pacing.
 *
 * Timeline:
 *   0-40:    Hook headline
 *   40-70:   Subline
 *   70-250:  GitHub screenshot (6 seconds of just the screenshot to read)
 *   260-380: "By day / by night" cards
 *   390-460: Bridge statement + projects
 *   470-600: Priyanka moment
 *   600-660: Hold
 */

export const TechStackScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  // Phase 1: Hook (0-70)
  const hookOpacity = interpolate(frame, [5, 25], [0, 1], { extrapolateRight: "clamp" });
  const hookY = spring({ frame: frame - 5, fps, from: 16, to: 0, durationInFrames: 22 });
  const sublineOpacity = interpolate(frame, [40, 60], [0, 1], { extrapolateRight: "clamp" });

  // Phase 2: GitHub screenshot (70-260) — 6+ seconds of hold
  const screenshotOpacity = interpolate(frame, [70, 95], [0, 1], { extrapolateRight: "clamp" });
  const screenshotScale = spring({ frame: frame - 70, fps, from: 0.95, to: 1, durationInFrames: 25 });
  // Screenshot slides up slightly as content appears below
  const screenshotY = interpolate(frame, [260, 310], [0, -30], { extrapolateRight: "clamp", extrapolateLeft: "clamp" });
  const screenshotShrink = interpolate(frame, [260, 310], [1, 0.75], { extrapolateRight: "clamp", extrapolateLeft: "clamp" });

  // Phase 3: "By day / by night" (280-400)
  const byDayOpacity = interpolate(frame, [280, 305], [0, 1], { extrapolateRight: "clamp" });
  const byNightOpacity = interpolate(frame, [310, 335], [0, 1], { extrapolateRight: "clamp" });

  // Phase 4: Bridge + projects (380-480)
  const bridgeOpacity = interpolate(frame, [380, 405], [0, 1], { extrapolateRight: "clamp" });
  const projectsOpacity = interpolate(frame, [420, 445], [0, 1], { extrapolateRight: "clamp" });

  // Phase 5: Priyanka (500-660)
  const priyankaOpacity = interpolate(frame, [500, 530], [0, 1], { extrapolateRight: "clamp" });
  const priyankaY = spring({ frame: frame - 500, fps, from: 12, to: 0, durationInFrames: 22 });

  return (
    <AbsoluteFill
      style={{
        background: `linear-gradient(160deg, #0D1117 0%, #161B22 50%, #0D1117 100%)`,
        fontFamily: fonts.heading,
        padding: "28px 60px",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      {/* ── Hook ── */}
      <div
        style={{
          opacity: hookOpacity,
          transform: `translateY(${hookY}px)`,
          textAlign: "center",
          marginBottom: 4,
        }}
      >
        <h2 style={{ fontSize: 38, fontWeight: 700, color: "#FFFFFF", margin: 0 }}>
          There&apos;s more to me than the sales title.
        </h2>
      </div>
      <p
        style={{
          fontSize: 18,
          fontWeight: 400,
          color: "#8B949E",
          margin: "0 0 14px 0",
          textAlign: "center",
          opacity: sublineOpacity,
        }}
      >
        I&apos;ve been building and shipping software for years.
      </p>

      {/* ── GitHub screenshot — holds for 6 seconds before anything else appears ── */}
      <div
        style={{
          opacity: screenshotOpacity,
          transform: `scale(${screenshotScale * screenshotShrink}) translateY(${screenshotY}px)`,
          borderRadius: 14,
          overflow: "hidden",
          border: "1.5px solid #30363D",
          boxShadow: "0 6px 32px rgba(0,0,0,0.5)",
          maxWidth: 950,
          width: "100%",
          marginBottom: 14,
          transformOrigin: "top center",
        }}
      >
        <Img
          src={staticFile("github-profile.png")}
          style={{ width: "100%", display: "block" }}
        />
      </div>

      {/* ── By day / by night — appears after screenshot has been visible for 6s ── */}
      <div style={{ maxWidth: 880, width: "100%", marginBottom: 10 }}>
        <div style={{ display: "flex", gap: 24 }}>
          <div
            style={{
              flex: 1,
              opacity: byDayOpacity,
              padding: "12px 16px",
              borderRadius: 10,
              borderLeft: "3px solid #F7A600",
              backgroundColor: "rgba(247,166,0,0.05)",
            }}
          >
            <div style={{ fontSize: 11, fontWeight: 600, color: "#F7A600", textTransform: "uppercase", letterSpacing: 1.5, marginBottom: 4 }}>
              By Day
            </div>
            <p style={{ fontSize: 14, color: "#C9D1D9", margin: 0, lineHeight: 1.5 }}>
              I structure equipment deals, run credit analysis, build financial spreads,
              and model amortization schedules.
            </p>
          </div>

          <div
            style={{
              flex: 1,
              opacity: byNightOpacity,
              padding: "12px 16px",
              borderRadius: 10,
              borderLeft: `3px solid ${colors.primary}`,
              backgroundColor: `${colors.primary}08`,
            }}
          >
            <div style={{ fontSize: 11, fontWeight: 600, color: colors.primary, textTransform: "uppercase", letterSpacing: 1.5, marginBottom: 4 }}>
              By Night
            </div>
            <p style={{ fontSize: 14, color: "#C9D1D9", margin: 0, lineHeight: 1.5 }}>
              I build full-stack applications, study microservices architectures,
              and ship products with real users and real revenue.
            </p>
          </div>
        </div>

        {/* Bridge */}
        <p
          style={{
            opacity: bridgeOpacity,
            fontSize: 15,
            fontWeight: 500,
            color: "#8B949E",
            textAlign: "center",
            margin: "10px 0 0 0",
            lineHeight: 1.5,
          }}
        >
          These 6 months in sales have been some of the most meaningful growth I&apos;ve experienced —
          as an engineer <em>and</em> as a finance professional.
        </p>
      </div>

      {/* ── Proof points ── */}
      <div
        style={{
          opacity: projectsOpacity,
          display: "flex",
          justifyContent: "center",
          gap: 16,
          marginBottom: 12,
        }}
      >
        {[
          { label: "Applify AI", detail: "150+ paying users", color: "#A371F7" },
          { label: "Solstice Pilates", detail: "500+ users · $10K+ rev", color: colors.success },
          { label: "SKompXcel", detail: "80+ students mentored", color: "#F7A600" },
          { label: "Podcast Hub", detail: "RabbitMQ microservices", color: colors.primary },
        ].map((proj) => (
          <div
            key={proj.label}
            style={{
              padding: "8px 14px",
              borderRadius: 10,
              border: "1px solid #30363D",
              backgroundColor: "#0D1117",
              minWidth: 160,
              textAlign: "center",
            }}
          >
            <div style={{ fontSize: 13, fontWeight: 600, color: "#C9D1D9", marginBottom: 2 }}>
              {proj.label}
            </div>
            <div style={{ fontSize: 11, fontWeight: 600, color: proj.color }}>{proj.detail}</div>
          </div>
        ))}
      </div>

      {/* ── Priyanka ── */}
      <div
        style={{
          opacity: priyankaOpacity,
          transform: `translateY(${priyankaY}px)`,
          textAlign: "center",
          padding: "12px 28px",
          borderRadius: 12,
          border: `1.5px solid ${colors.primary}40`,
          backgroundColor: `${colors.primary}10`,
        }}
      >
        <p style={{ fontSize: 17, fontWeight: 500, color: "#58A6FF", margin: 0, lineHeight: 1.5 }}>
          When Priyanka reached out and mentioned there might be an opportunity —
          I had to jump on it.
        </p>
      </div>
    </AbsoluteFill>
  );
};
