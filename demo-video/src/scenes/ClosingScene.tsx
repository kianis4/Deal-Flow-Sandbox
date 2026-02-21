import { AbsoluteFill, interpolate, useCurrentFrame, spring, useVideoConfig } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 7: Closing — humble, eager, forward-looking.
 * "This is a simple mockup. But I have many more ideas."
 */
export const ClosingScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const line1Opacity = interpolate(frame, [10, 30], [0, 1], { extrapolateRight: "clamp" });
  const line1Y = spring({ frame: frame - 10, fps, from: 20, to: 0, durationInFrames: 25 });

  const dividerWidth = spring({ frame: frame - 35, fps, from: 0, to: 300, durationInFrames: 35 });

  const line2Opacity = interpolate(frame, [50, 75], [0, 1], { extrapolateRight: "clamp" });

  const ideasOpacity = interpolate(frame, [90, 115], [0, 1], { extrapolateRight: "clamp" });

  const askOpacity = interpolate(frame, [150, 175], [0, 1], { extrapolateRight: "clamp" });
  const askY = spring({ frame: frame - 150, fps, from: 15, to: 0, durationInFrames: 25 });

  const statsOpacity = interpolate(frame, [210, 240], [0, 1], { extrapolateRight: "clamp" });

  const contactOpacity = interpolate(frame, [280, 310], [0, 1], { extrapolateRight: "clamp" });

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
      <div style={{ textAlign: "center", maxWidth: 950 }}>
        {/* Humble framing */}
        <p
          style={{
            fontSize: 20,
            fontWeight: 400,
            color: "#667788",
            margin: "0 0 12px 0",
            opacity: line1Opacity,
            transform: `translateY(${line1Y}px)`,
          }}
        >
          I know this is a simple mockup —
        </p>

        <h1
          style={{
            fontSize: 48,
            fontWeight: 700,
            color: "#FFFFFF",
            margin: "0 0 8px 0",
            opacity: line1Opacity,
            transform: `translateY(${line1Y}px)`,
            lineHeight: 1.2,
          }}
        >
          but this is just one idea, and I have many more.
        </h1>

        <div
          style={{
            width: dividerWidth,
            height: 2,
            backgroundColor: colors.primary,
            margin: "16px auto 24px auto",
            borderRadius: 1,
          }}
        />

        {/* Context — what it saves */}
        <p
          style={{
            fontSize: 22,
            fontWeight: 400,
            color: "#AAB8C8",
            margin: "0 0 30px 0",
            opacity: line2Opacity,
            lineHeight: 1.5,
          }}
        >
          Even this one feature — the exposure lookup — saves 15–20 minutes
          <br />
          of manual work before we even decide to submit an application.
        </p>

        {/* What I want */}
        <div
          style={{
            opacity: ideasOpacity,
            display: "flex",
            justifyContent: "center",
            gap: 28,
            marginBottom: 36,
          }}
        >
          {[
            "I\u2019m genuinely eager to learn what you\u2019re working on",
            "I want to understand the systems, the stack, the challenges",
            "And show my dedication to contributing and growing",
          ].map((idea, i) => (
            <div
              key={idea}
              style={{
                display: "flex",
                alignItems: "flex-start",
                gap: 10,
                maxWidth: 280,
                textAlign: "left",
              }}
            >
              <div
                style={{
                  width: 22,
                  height: 22,
                  borderRadius: 11,
                  backgroundColor: `${colors.primary}30`,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  fontSize: 12,
                  fontWeight: 700,
                  color: colors.primary,
                  flexShrink: 0,
                  marginTop: 2,
                }}
              >
                {i + 1}
              </div>
              <span style={{ fontSize: 16, color: "#8899AA", lineHeight: 1.4 }}>{idea}</span>
            </div>
          ))}
        </div>

        {/* The ask */}
        <div
          style={{
            opacity: askOpacity,
            transform: `translateY(${askY}px)`,
            padding: "18px 40px",
            borderRadius: 16,
            border: `1.5px solid ${colors.primary}40`,
            backgroundColor: `${colors.primary}08`,
            marginBottom: 32,
            display: "inline-block",
          }}
        >
          <p
            style={{
              fontSize: 24,
              fontWeight: 600,
              color: "#FFFFFF",
              margin: 0,
              lineHeight: 1.5,
            }}
          >
            I&apos;d love to meet and discuss the opportunity.
          </p>
        </div>

        {/* Proof points — humble but credible */}
        <div
          style={{
            opacity: statsOpacity,
            display: "flex",
            justifyContent: "center",
            gap: 45,
            marginBottom: 36,
          }}
        >
          {[
            { value: "2 nights", label: "to build this prototype" },
            { value: "CAS 735", label: "Microservices — McMaster (strong rec. available)" },
            { value: "2,521", label: "GitHub contributions this year" },
            { value: "$20M+", label: "deals financed at MHCC" },
          ].map((stat) => (
            <div key={stat.label} style={{ textAlign: "center", maxWidth: 180 }}>
              <div
                style={{
                  fontSize: 26,
                  fontWeight: 700,
                  color: colors.primary,
                  fontFamily: fonts.mono,
                  lineHeight: 1,
                }}
              >
                {stat.value}
              </div>
              <div style={{ fontSize: 12, color: "#556677", marginTop: 6, lineHeight: 1.3 }}>{stat.label}</div>
            </div>
          ))}
        </div>

        {/* Contact */}
        <div style={{ opacity: contactOpacity }}>
          <p
            style={{
              fontSize: 18,
              fontWeight: 600,
              color: colors.primary,
              letterSpacing: 3,
              textTransform: "uppercase",
              margin: "0 0 10px 0",
            }}
          >
            Suleyman Kiani
          </p>
          <div
            style={{
              display: "flex",
              justifyContent: "center",
              gap: 24,
              fontSize: 14,
              color: "#667788",
            }}
          >
            {[
              "suleyman.io",
              "github.com/kianis4",
              "linkedin.com/in/suleyman-kiani",
              "kianis4@mcmaster.ca",
            ].map((link) => (
              <span key={link}>{link}</span>
            ))}
          </div>
          <p
            style={{
              fontSize: 13,
              color: "#445566",
              margin: "10px 0 0 0",
            }}
          >
            BASc Computer Science · MEng Computing &amp; Software (in progress) · Mitsubishi HC Capital Canada
          </p>
        </div>
      </div>
    </AbsoluteFill>
  );
};
