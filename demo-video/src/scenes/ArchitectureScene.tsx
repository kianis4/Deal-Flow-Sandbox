import { AbsoluteFill, interpolate, useCurrentFrame, spring, useVideoConfig } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 4: Architecture — proper visual diagram with data flow,
 * message broker, database, and thought process annotations.
 * Extended to 480 frames (16s) for the full architecture story.
 */

/* ─── small reusable pieces ─── */

const ServiceNode: React.FC<{
  label: string;
  type: string;
  port?: string;
  color: string;
  x: number;
  y: number;
  opacity: number;
  width?: number;
}> = ({ label, type, port, color, x, y, opacity, width = 220 }) => (
  <div
    style={{
      position: "absolute",
      left: x,
      top: y,
      width,
      opacity,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
    }}
  >
    {/* Service box */}
    <div
      style={{
        width: "100%",
        padding: "14px 16px",
        borderRadius: 12,
        backgroundColor: "#FFFFFF",
        border: `2.5px solid ${color}`,
        boxShadow: "0 3px 16px rgba(0,0,0,0.06)",
        textAlign: "center",
      }}
    >
      <div style={{ fontSize: 11, fontWeight: 600, color, textTransform: "uppercase", letterSpacing: 1.5, marginBottom: 3 }}>
        {type}
      </div>
      <div style={{ fontSize: 17, fontWeight: 700, color: colors.textPrimary }}>
        {label}
      </div>
      {port && (
        <div style={{ fontSize: 11, color: colors.textMuted, fontFamily: fonts.mono, marginTop: 2 }}>
          :{port}
        </div>
      )}
    </div>
  </div>
);

const InfraNode: React.FC<{
  label: string;
  sublabel: string;
  color: string;
  x: number;
  y: number;
  opacity: number;
  width?: number;
  icon?: string;
}> = ({ label, sublabel, color, x, y, opacity, width = 180, icon }) => (
  <div
    style={{
      position: "absolute",
      left: x,
      top: y,
      width,
      opacity,
      textAlign: "center",
    }}
  >
    <div
      style={{
        padding: "12px 14px",
        borderRadius: 10,
        backgroundColor: `${color}08`,
        border: `1.5px dashed ${color}60`,
      }}
    >
      {icon && <div style={{ fontSize: 22, marginBottom: 2 }}>{icon}</div>}
      <div style={{ fontSize: 14, fontWeight: 700, color }}>{label}</div>
      <div style={{ fontSize: 11, color: colors.textMuted }}>{sublabel}</div>
    </div>
  </div>
);

const FlowLine: React.FC<{
  x1: number; y1: number; x2: number; y2: number;
  label: string;
  opacity: number;
  color?: string;
  dashed?: boolean;
}> = ({ x1, y1, x2, y2, label, opacity, color = colors.primary, dashed = false }) => {
  const dx = x2 - x1;
  const dy = y2 - y1;
  const length = Math.sqrt(dx * dx + dy * dy);
  const angle = Math.atan2(dy, dx) * (180 / Math.PI);

  return (
    <div style={{ position: "absolute", left: x1, top: y1, opacity }}>
      {/* Line */}
      <div
        style={{
          width: length,
          height: 0,
          borderTop: dashed ? `2px dashed ${color}50` : `2px solid ${color}`,
          transform: `rotate(${angle}deg)`,
          transformOrigin: "0 0",
        }}
      />
      {/* Arrow tip */}
      <div
        style={{
          position: "absolute",
          left: x2 - x1 - 4,
          top: y2 - y1 - 4,
          width: 0,
          height: 0,
          borderLeft: `7px solid ${color}`,
          borderTop: "4px solid transparent",
          borderBottom: "4px solid transparent",
          transform: `rotate(${angle}deg)`,
        }}
      />
      {/* Label */}
      <div
        style={{
          position: "absolute",
          left: dx / 2 - 50,
          top: dy / 2 - (dy > 10 ? 8 : -16),
          width: 100,
          textAlign: "center",
          fontSize: 11,
          fontWeight: 600,
          color,
          fontFamily: fonts.mono,
          backgroundColor: colors.bgLight,
          padding: "1px 6px",
          borderRadius: 4,
        }}
      >
        {label}
      </div>
    </div>
  );
};

const Annotation: React.FC<{
  text: string;
  x: number;
  y: number;
  opacity: number;
  align?: "left" | "center" | "right";
  maxWidth?: number;
}> = ({ text, x, y, opacity, align = "left", maxWidth = 220 }) => (
  <div
    style={{
      position: "absolute",
      left: x,
      top: y,
      opacity,
      maxWidth,
      textAlign: align,
    }}
  >
    <div
      style={{
        fontSize: 12,
        fontStyle: "italic",
        color: colors.primary,
        lineHeight: 1.4,
        padding: "4px 8px",
        borderLeft: `2px solid ${colors.primary}40`,
        backgroundColor: `${colors.primary}05`,
      }}
    >
      {text}
    </div>
  </div>
);

export const ArchitectureScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const headerOpacity = interpolate(frame, [0, 20], [0, 1], { extrapolateRight: "clamp" });
  const headerY = spring({ frame, fps, from: 18, to: 0, durationInFrames: 22 });

  const sp = (delay: number) => spring({ frame: frame - delay, fps, from: 0, to: 1, durationInFrames: 20 });

  // Phase 1: Services appear (frames 25-110)
  const intakeOp = sp(25);
  const rabbitOp = sp(50);
  const scoringOp = sp(70);
  const notifyOp = sp(90);
  const reportingOp = sp(110);

  // Phase 2: Flow arrows (frames 60-140)
  const arrow1 = sp(60);
  const arrow2 = sp(85);
  const arrow3 = sp(105);
  const arrow4 = sp(125);
  const arrow5 = sp(140);

  // Phase 3: Database layer (frames 130-160)
  const dbOp = sp(130);
  const dbArrow1 = sp(145);
  const dbArrow2 = sp(155);

  // Phase 4: Annotations / thought process (frames 180-280)
  const ann1 = interpolate(frame, [180, 200], [0, 1], { extrapolateRight: "clamp" });
  const ann2 = interpolate(frame, [210, 230], [0, 1], { extrapolateRight: "clamp" });
  const ann3 = interpolate(frame, [240, 260], [0, 1], { extrapolateRight: "clamp" });

  // Phase 5: Bottom context bar (frames 300-340)
  const contextOp = interpolate(frame, [300, 325], [0, 1], { extrapolateRight: "clamp" });

  // Phase 6: Tech badges (frames 340-380)
  const badgesOp = interpolate(frame, [340, 365], [0, 1], { extrapolateRight: "clamp" });

  return (
    <AbsoluteFill
      style={{
        background: colors.bgLight,
        fontFamily: fonts.heading,
        padding: "30px 60px",
      }}
    >
      {/* Header */}
      <div
        style={{
          opacity: headerOpacity,
          transform: `translateY(${headerY}px)`,
          textAlign: "center",
          marginBottom: 8,
        }}
      >
        <p style={{ fontSize: 15, fontWeight: 600, color: colors.primary, letterSpacing: 2, textTransform: "uppercase", margin: "0 0 4px 0" }}>
          What I Built
        </p>
        <h2 style={{ fontSize: 38, fontWeight: 700, color: colors.textPrimary, margin: 0 }}>
          4 Microservices · Hexagonal Architecture · Event-Driven
        </h2>
      </div>

      {/* ─── Main diagram area ─── */}
      <div style={{ position: "relative", width: "100%", height: 620 }}>

        {/* ── Row 1: Intake API ── */}
        <ServiceNode label="Intake API" type="HTTP · Minimal API" port="5001" color={colors.intakeApi} x={60} y={30} opacity={intakeOp} />

        {/* ── RabbitMQ broker (center) ── */}
        <InfraNode label="RabbitMQ" sublabel="Message Broker" color="#FF6600" x={370} y={35} opacity={rabbitOp} width={160} />

        {/* ── Row 1: Scoring Worker ── */}
        <ServiceNode label="Scoring Worker" type="Consumer · Worker Service" color={colors.scoringWorker} x={620} y={30} opacity={scoringOp} />

        {/* ── Row 2: Notify Worker ── */}
        <ServiceNode label="Notify Worker" type="Consumer · Worker Service" color={colors.notifyWorker} x={620} y={200} opacity={notifyOp} />

        {/* ── Row 2: Reporting API ── */}
        <ServiceNode label="Reporting API" type="HTTP · Minimal API" port="5002" color={colors.reportingApi} x={60} y={200} opacity={reportingOp} width={220} />

        {/* ── Flow arrows ── */}
        {/* Intake → RabbitMQ */}
        <FlowLine x1={285} y1={65} x2={368} y2={65} label="publish" opacity={arrow1} />
        {/* RabbitMQ → Scoring */}
        <FlowLine x1={533} y1={65} x2={618} y2={65} label="deal.submitted" opacity={arrow2} />
        {/* Scoring → RabbitMQ (back) */}
        <FlowLine x1={618} y1={100} x2={533} y2={100} label="deal.scored" opacity={arrow3} color={colors.scoringWorker} />
        {/* RabbitMQ → Notify */}
        <FlowLine x1={450} y1={105} x2={620} y2={230} label="consume" opacity={arrow4} color={colors.notifyWorker} />
        {/* Reporting reads DB (shown below) */}

        {/* ── Database layer ── */}
        <InfraNode label="PostgreSQL 16" sublabel="Deals · Events · Audit Trails" color="#336791" x={320} y={200} opacity={dbOp} width={200} icon="" />

        {/* Intake → DB */}
        <FlowLine x1={170} y1={110} x2={380} y2={200} label="persist" opacity={dbArrow1} color="#336791" dashed />
        {/* DB → Reporting */}
        <FlowLine x1={380} y1={255} x2={280} y2={240} label="read" opacity={dbArrow2} color={colors.reportingApi} dashed />
        {/* Scoring → DB */}
        <FlowLine x1={680} y1={110} x2={480} y2={200} label="update score" opacity={sp(160)} color={colors.scoringWorker} dashed />

        {/* ── Docker Compose boundary ── */}
        <div
          style={{
            position: "absolute",
            left: 20,
            top: 0,
            right: 20,
            bottom: 270,
            border: `1.5px dashed ${colors.border}`,
            borderRadius: 16,
            opacity: sp(140),
          }}
        >
          <div style={{ position: "absolute", top: -12, right: 20, fontSize: 11, fontWeight: 600, color: colors.textMuted, backgroundColor: colors.bgLight, padding: "0 8px" }}>
            Docker Compose
          </div>
        </div>

        {/* ── Thought process annotations ── */}
        <Annotation
          text="Decoupled: Intake publishes and returns immediately. Scoring happens async — no blocking."
          x={900} y={30} opacity={ann1} maxWidth={250}
        />
        <Annotation
          text="Pure function: ScoringEngine.Score() has zero DB dependencies — trivially unit-testable."
          x={900} y={120} opacity={ann2} maxWidth={250}
        />
        <Annotation
          text="Audit trail: Every state change writes to DealEvents with full JSONB payload — no event-sourcing complexity."
          x={900} y={210} opacity={ann3} maxWidth={250}
        />

        {/* ── Data flow summary (bottom section) ── */}
        <div
          style={{
            position: "absolute",
            bottom: 100,
            left: 0,
            right: 0,
            opacity: contextOp,
          }}
        >
          {/* Flow diagram: linear pipeline */}
          <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: 8, marginBottom: 16 }}>
            {[
              { label: "POST /deals", color: colors.intakeApi },
              { label: "Validate", color: colors.intakeApi },
              { label: "Persist", color: "#336791" },
              { label: "Publish Event", color: "#FF6600" },
              { label: "Score (async)", color: colors.scoringWorker },
              { label: "Update Status", color: colors.scoringWorker },
              { label: "Notify (async)", color: colors.notifyWorker },
              { label: "Query / Report", color: colors.reportingApi },
            ].map((step, i) => (
              <div key={step.label} style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <div
                  style={{
                    padding: "5px 10px",
                    borderRadius: 6,
                    backgroundColor: `${step.color}12`,
                    border: `1px solid ${step.color}30`,
                    fontSize: 11,
                    fontWeight: 600,
                    color: step.color,
                    whiteSpace: "nowrap",
                  }}
                >
                  {step.label}
                </div>
                {i < 7 && <span style={{ fontSize: 14, color: colors.textMuted }}>→</span>}
              </div>
            ))}
          </div>

          <p style={{ textAlign: "center", fontSize: 14, color: colors.textSecondary, margin: 0 }}>
            Full deal lifecycle: submit → validate → persist → score → notify → query — all asynchronous, all audited
          </p>
        </div>

        {/* ── Context row: PostgreSQL mock + Hexagonal + CAS 735 ── */}
        <div
          style={{
            position: "absolute",
            bottom: 25,
            left: 0,
            right: 0,
            display: "flex",
            justifyContent: "center",
            gap: 40,
            opacity: contextOp,
          }}
        >
          {[
            { top: "PostgreSQL Mock", bottom: "Modeled from Vision deal structure", color: "#336791" },
            { top: "Hexagonal Architecture", bottom: "Ports & adapters · decoupled domains", color: colors.scoringWorker },
            { top: "CAS 735 — McMaster", bottom: "Microservices-Oriented Architectures", color: colors.primary },
          ].map((item) => (
            <div key={item.top} style={{ display: "flex", alignItems: "center", gap: 10 }}>
              <div style={{ width: 8, height: 8, borderRadius: 4, backgroundColor: item.color }} />
              <div>
                <div style={{ fontSize: 14, fontWeight: 600, color: colors.textPrimary }}>{item.top}</div>
                <div style={{ fontSize: 11, color: colors.textMuted }}>{item.bottom}</div>
              </div>
            </div>
          ))}
        </div>

        {/* ── Tech badges ── */}
        <div
          style={{
            position: "absolute",
            bottom: -10,
            left: 0,
            right: 0,
            display: "flex",
            justifyContent: "center",
            gap: 16,
            opacity: badgesOp,
          }}
        >
          {[
            { name: ".NET 10", color: "#512BD4" },
            { name: "MassTransit 8.5", color: "#6B46C1" },
            { name: "EF Core 10", color: "#512BD4" },
            { name: "Docker Compose", color: "#2496ED" },
          ].map((tech) => (
            <div
              key={tech.name}
              style={{
                padding: "4px 14px",
                borderRadius: 16,
                border: `1px solid ${colors.border}`,
                backgroundColor: colors.bgWhite,
                fontSize: 12,
                fontWeight: 500,
                color: colors.textSecondary,
                display: "flex",
                alignItems: "center",
                gap: 6,
              }}
            >
              <div style={{ width: 6, height: 6, borderRadius: 3, backgroundColor: tech.color }} />
              {tech.name}
            </div>
          ))}
        </div>
      </div>
    </AbsoluteFill>
  );
};
