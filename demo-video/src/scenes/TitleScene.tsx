import { AbsoluteFill, interpolate, useCurrentFrame, spring, useVideoConfig } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 1: Personal hook — not just "I work in sales" but the full picture.
 * BASc CS, MEng student, Account Manager at MHCC, builder.
 */
export const TitleScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const nameOpacity = interpolate(frame, [8, 25], [0, 1], { extrapolateRight: "clamp" });
  const nameY = spring({ frame: frame - 8, fps, from: 25, to: 0, durationInFrames: 22 });

  const roleOpacity = interpolate(frame, [35, 55], [0, 1], { extrapolateRight: "clamp" });
  const roleY = spring({ frame: frame - 35, fps, from: 18, to: 0, durationInFrames: 22 });

  const statementOpacity = interpolate(frame, [75, 100], [0, 1], { extrapolateRight: "clamp" });

  const accentWidth = spring({ frame: frame - 5, fps, from: 0, to: 140, durationInFrames: 35 });

  // Credential pills
  const pillsOpacity = interpolate(frame, [110, 135], [0, 1], { extrapolateRight: "clamp" });

  return (
    <AbsoluteFill
      style={{
        background: `linear-gradient(160deg, #FAFBFD 0%, #EEF2F7 40%, #E3EAF3 100%)`,
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        fontFamily: fonts.heading,
      }}
    >
      {/* Subtle top accent */}
      <div
        style={{
          position: "absolute",
          top: 0,
          left: 0,
          width: accentWidth,
          height: 4,
          backgroundColor: colors.primary,
        }}
      />

      <div style={{ textAlign: "center" }}>
        {/* Name */}
        <p
          style={{
            fontSize: 20,
            fontWeight: 600,
            color: colors.primary,
            letterSpacing: 4,
            textTransform: "uppercase",
            margin: "0 0 18px 0",
            opacity: nameOpacity,
            transform: `translateY(${nameY}px)`,
          }}
        >
          Suleyman Kiani
        </p>

        {/* Role + education */}
        <div
          style={{
            opacity: roleOpacity,
            transform: `translateY(${roleY}px)`,
          }}
        >
          <h1
            style={{
              fontSize: 52,
              fontWeight: 700,
              color: colors.textPrimary,
              margin: "0 0 10px 0",
              lineHeight: 1.2,
            }}
          >
            I work in equipment finance sales.
          </h1>
          <p
            style={{
              fontSize: 24,
              fontWeight: 400,
              color: colors.textSecondary,
              margin: "0 0 6px 0",
            }}
          >
            Sales Analyst — Mitsubishi HC Capital Canada
          </p>
          <p
            style={{
              fontSize: 20,
              fontWeight: 400,
              color: colors.textMuted,
              margin: 0,
            }}
          >
            BASc Computer Science · MEng Computing &amp; Software (in progress)
          </p>
        </div>

        {/* Statement */}
        <p
          style={{
            fontSize: 28,
            fontWeight: 500,
            color: colors.textPrimary,
            margin: "36px 0 0 0",
            opacity: statementOpacity,
            maxWidth: 750,
            lineHeight: 1.5,
            display: "inline-block",
          }}
        >
          I use these systems every day.
          <br />
          I see where they slow us down — and I have ideas.
        </p>

        {/* Credential pills */}
        <div
          style={{
            display: "flex",
            justifyContent: "center",
            gap: 16,
            marginTop: 36,
            opacity: pillsOpacity,
          }}
        >
          {[
            { text: "$20M+ deals financed", color: colors.success },
            { text: "2,521 GitHub contributions", color: colors.primary },
            { text: "150+ Applify AI users", color: colors.scoringWorker },
            { text: "McMaster MEng (in progress)", color: colors.warning },
          ].map((pill) => (
            <div
              key={pill.text}
              style={{
                padding: "7px 18px",
                borderRadius: 20,
                border: `1.5px solid ${pill.color}30`,
                backgroundColor: `${pill.color}08`,
                fontSize: 15,
                fontWeight: 600,
                color: pill.color,
              }}
            >
              {pill.text}
            </div>
          ))}
        </div>
      </div>
    </AbsoluteFill>
  );
};
