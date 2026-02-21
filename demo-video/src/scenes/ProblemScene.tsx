import { AbsoluteFill, interpolate, useCurrentFrame, spring, useVideoConfig } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 2: The daily reality — recreate Vision + SSRS visually
 * Shows the two tools side by side with the pain points highlighted
 */

const VisionMockRow: React.FC<{
  appNum: string;
  status: string;
  statusColor: string;
  customer: string;
  vendor: string;
  format: string;
  opacity: number;
}> = ({ appNum, status, statusColor, customer, vendor, format, opacity }) => (
  <div
    style={{
      opacity,
      display: "flex",
      alignItems: "center",
      padding: "8px 12px",
      borderBottom: "1px solid #E5E5E5",
      fontSize: 13,
      gap: 8,
    }}
  >
    <div style={{ width: 60, color: "#666", fontFamily: fonts.mono }}>{appNum}</div>
    <div style={{ width: 130 }}>
      <span
        style={{
          fontSize: 11,
          fontWeight: 600,
          color: statusColor,
          padding: "2px 8px",
          borderRadius: 3,
          backgroundColor: statusColor + "15",
        }}
      >
        {status}
      </span>
    </div>
    <div style={{ width: 200, fontWeight: 500, color: "#333", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
      {customer}
    </div>
    <div style={{ width: 220, color: "#666", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{vendor}</div>
    <div style={{ width: 80, color: "#888" }}>{format}</div>
  </div>
);

export const ProblemScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  // Phase 1: Vision (frames 0-160)
  const visionLabelOpacity = interpolate(frame, [0, 15], [0, 1], { extrapolateRight: "clamp" });
  const visionOpacity = interpolate(frame, [10, 30], [0, 1], { extrapolateRight: "clamp" });
  const visionCalloutOpacity = interpolate(frame, [70, 90], [0, 1], { extrapolateRight: "clamp" });

  // Phase 2: SSRS (frames 160-360)
  const ssrsLabelOpacity = interpolate(frame, [160, 175], [0, 1], { extrapolateRight: "clamp" });
  const ssrsOpacity = interpolate(frame, [170, 190], [0, 1], { extrapolateRight: "clamp" });
  const ssrsCalloutOpacity = interpolate(frame, [230, 250], [0, 1], { extrapolateRight: "clamp" });

  // Timer animation for SSRS pain point
  const timerActive = frame >= 250;
  const timerProgress = interpolate(frame, [250, 340], [0, 1], { extrapolateRight: "clamp" });
  const timerMinutes = Math.floor(timerProgress * 18);
  const timerSec = Math.floor((timerProgress * 18 * 60) % 60);

  const visionRows = [
    { appNum: "119948", status: "Credit Validation", statusColor: "#D48806", customer: "3D Prints Canada Inc.", vendor: "Remorques GC Inc. DBA Groupe Carrier", format: "EF Vendor" },
    { appNum: "119947", status: "Missing Info", statusColor: "#CF1322", customer: "HABITATIONS MDC INC.", vendor: "9184-3771 Quebec Inc. DBA Groupe...", format: "EF Vendor" },
    { appNum: "119946", status: "Credit Validation", statusColor: "#D48806", customer: "Acier Québec-Maritimes inc.", vendor: "LE CENTRE ROUTIER (1994) INC.", format: "EF Vendor" },
    { appNum: "119945", status: "Credit Validation", statusColor: "#D48806", customer: "Roger Maurice Nantel", vendor: "9383-3068 Quebec Inc. DBA Élite C3", format: "EF Broker" },
    { appNum: "119944", status: "Autoscoring Approved", statusColor: "#389E0D", customer: "ASTORI RÉNOVATION INC.", vendor: "VR THETFORD INC.", format: "EF Vendor" },
    { appNum: "119943", status: "Credit Validation", statusColor: "#D48806", customer: "Gibeau excavation inc", vendor: "ATTACHES CHÂTEAUGUAY INC.", format: "EF Vendor" },
    { appNum: "119942", status: "Credit Validation", statusColor: "#D48806", customer: "vision propre inc.", vendor: "CENTRE DU CAMION MONT-LAURIER...", format: "EF Vendor" },
    { appNum: "119941", status: "Credit Validation", statusColor: "#D48806", customer: "9205-0046 QUÉBEC INC.", vendor: "CENTRE DU CAMION BEAUDOIN INC.", format: "EF Vendor" },
  ];

  return (
    <AbsoluteFill
      style={{
        background: colors.bgLight,
        fontFamily: fonts.heading,
        padding: "40px 60px",
      }}
    >
      {/* Two-panel layout */}
      <div style={{ display: "flex", gap: 40, height: "100%" }}>
        {/* LEFT: Vision mock */}
        <div style={{ flex: 1, display: "flex", flexDirection: "column" }}>
          <div
            style={{
              opacity: visionLabelOpacity,
              fontSize: 14,
              fontWeight: 600,
              color: colors.primary,
              letterSpacing: 2,
              textTransform: "uppercase",
              marginBottom: 8,
            }}
          >
            Every day I open this
          </div>

          {/* Vision window */}
          <div
            style={{
              opacity: visionOpacity,
              flex: 1,
              borderRadius: 12,
              overflow: "hidden",
              border: `1.5px solid ${colors.border}`,
              backgroundColor: "#FFF",
              boxShadow: "0 4px 20px rgba(0,0,0,0.08)",
              display: "flex",
              flexDirection: "column",
            }}
          >
            {/* Vision nav bar */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                padding: "8px 16px",
                backgroundColor: "#F8F8F8",
                borderBottom: "1px solid #E0E0E0",
                gap: 20,
              }}
            >
              <span style={{ fontSize: 18, fontWeight: 700, color: "#E67E22" }}>Vision</span>
              {["Master Records", "Transactions", "Configuration", "Resources"].map((item) => (
                <span key={item} style={{ fontSize: 12, color: "#666" }}>{item}</span>
              ))}
              <div style={{ marginLeft: "auto", fontSize: 12, color: "#888" }}>skiani</div>
            </div>

            {/* Application List header */}
            <div
              style={{
                padding: "10px 16px",
                borderBottom: "1px solid #E8E8E8",
                display: "flex",
                alignItems: "center",
                gap: 12,
              }}
            >
              <span style={{ fontSize: 15, fontWeight: 600, color: "#333" }}>Application List</span>
              <span
                style={{
                  fontSize: 11,
                  padding: "3px 10px",
                  borderRadius: 4,
                  backgroundColor: "#E67E22",
                  color: "#FFF",
                  fontWeight: 600,
                }}
              >
                New Application
              </span>
            </div>

            {/* Column headers */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                padding: "6px 12px",
                backgroundColor: "#FAFAFA",
                borderBottom: "2px solid #E0E0E0",
                fontSize: 11,
                fontWeight: 600,
                color: "#D48806",
                gap: 8,
              }}
            >
              <div style={{ width: 60 }}>App. #</div>
              <div style={{ width: 130 }}>App. Status</div>
              <div style={{ width: 200 }}>Customer Legal Name</div>
              <div style={{ width: 220 }}>Primary Vendor</div>
              <div style={{ width: 80 }}>Format</div>
            </div>

            {/* Rows */}
            <div style={{ flex: 1, overflow: "hidden" }}>
              {visionRows.map((row, i) => {
                const rowOpacity = interpolate(frame, [30 + i * 5, 40 + i * 5], [0, 1], {
                  extrapolateRight: "clamp",
                });
                return <VisionMockRow key={row.appNum} {...row} opacity={rowOpacity} />;
              })}
            </div>
          </div>

          {/* Callout */}
          <div
            style={{
              opacity: visionCalloutOpacity,
              marginTop: 12,
              fontSize: 18,
              fontWeight: 500,
              color: colors.textSecondary,
              textAlign: "center",
            }}
          >
            <strong style={{ color: colors.textPrimary }}>Vision</strong> — our deal management system
          </div>
        </div>

        {/* RIGHT: SSRS mock */}
        <div style={{ flex: 1, display: "flex", flexDirection: "column" }}>
          <div
            style={{
              opacity: ssrsLabelOpacity,
              fontSize: 14,
              fontWeight: 600,
              color: colors.primary,
              letterSpacing: 2,
              textTransform: "uppercase",
              marginBottom: 8,
            }}
          >
            Then I open this
          </div>

          {/* SSRS window */}
          <div
            style={{
              opacity: ssrsOpacity,
              flex: 1,
              borderRadius: 12,
              overflow: "hidden",
              border: `1.5px solid ${colors.border}`,
              backgroundColor: "#FFF",
              boxShadow: "0 4px 20px rgba(0,0,0,0.08)",
              display: "flex",
              flexDirection: "column",
            }}
          >
            {/* SSRS header */}
            <div
              style={{
                display: "flex",
                alignItems: "center",
                padding: "8px 16px",
                backgroundColor: "#1A3A5C",
                gap: 12,
              }}
            >
              <div
                style={{
                  width: 20,
                  height: 20,
                  backgroundColor: "#C00",
                  borderRadius: 3,
                }}
              />
              <span style={{ fontSize: 13, color: "#CCC" }}>SQL Server Reporting Services</span>
              <span style={{ fontSize: 12, color: "#99B" }}>Home &gt; Canada &gt; Reports &gt;</span>
              <span style={{ fontSize: 12, color: "#FFF", fontWeight: 600 }}>IFL_ALL_DEAL_ACTIVE</span>
              <div style={{ marginLeft: "auto", fontSize: 12, color: "#AAC" }}>Suleyman Kiani</div>
            </div>

            {/* Search params */}
            <div
              style={{
                padding: "16px 20px",
                borderBottom: "1px solid #E0E0E0",
                display: "flex",
                gap: 24,
                alignItems: "center",
              }}
            >
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <span style={{ fontSize: 13, fontWeight: 600, color: "#333" }}>CLIENT NAME:</span>
                <div
                  style={{
                    padding: "4px 12px",
                    border: "1px solid #CCC",
                    borderRadius: 3,
                    fontSize: 13,
                    backgroundColor: "#FFFFF0",
                    color: "#333",
                    minWidth: 180,
                  }}
                >
                  Randhawa Freightways Ltd.
                </div>
              </div>
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <span style={{ fontSize: 13, fontWeight: 600, color: "#333" }}>DEALER NAME:</span>
                <div
                  style={{
                    padding: "4px 12px",
                    border: "1px solid #CCC",
                    borderRadius: 3,
                    fontSize: 13,
                    minWidth: 160,
                    color: "#999",
                  }}
                >
                  &nbsp;
                </div>
              </div>
              <div
                style={{
                  padding: "4px 14px",
                  backgroundColor: "#F0F0F0",
                  border: "1px solid #CCC",
                  borderRadius: 3,
                  fontSize: 12,
                  fontWeight: 600,
                  color: "#333",
                }}
              >
                View Report
              </div>
            </div>

            {/* Results table */}
            <div style={{ padding: "16px 20px", flex: 1 }}>
              {/* Header row */}
              <div
                style={{
                  display: "flex",
                  backgroundColor: "#5A5A5A",
                  color: "#FFF",
                  fontSize: 10,
                  fontWeight: 700,
                  padding: "6px 8px",
                  gap: 4,
                }}
              >
                {["CIEDEAL", "CODE CIE", "CUST ID", "REP", "TEAMTYPE", "Credit Risk", "REGION", "EQUIP COST"].map(
                  (h) => (
                    <div key={h} style={{ flex: 1, textAlign: "center" }}>{h}</div>
                  )
                )}
              </div>
              {/* Data row */}
              <div
                style={{
                  display: "flex",
                  fontSize: 11,
                  padding: "8px 8px",
                  borderBottom: "1px solid #E0E0E0",
                  gap: 4,
                  color: "#333",
                }}
              >
                {[
                  "Mitsubishi HC Capital Canada Crédit-Bail, Inc.",
                  "2",
                  "V118015",
                  "Edwin Van Schepen",
                  "VENDOR",
                  "CR2",
                  "ON",
                  "$56,400.00",
                ].map((v, i) => (
                  <div key={i} style={{ flex: 1, textAlign: "center", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{v}</div>
                ))}
              </div>

              {/* Manual process illustration */}
              <div
                style={{
                  marginTop: 40,
                  opacity: ssrsCalloutOpacity,
                  textAlign: "center",
                }}
              >
                <div style={{ fontSize: 15, color: "#999", marginBottom: 12 }}>
                  Then I manually scroll through pages, tally net invest, count NSFs...
                </div>
                <div style={{ display: "flex", justifyContent: "center", gap: 30 }}>
                  {["Scroll through pages", "Add up net invest", "Count NSFs manually", "Check last NSF date"].map((step, i) => (
                    <div
                      key={step}
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: 6,
                        opacity: interpolate(frame, [250 + i * 12, 262 + i * 12], [0, 1], { extrapolateRight: "clamp" }),
                      }}
                    >
                      <div
                        style={{
                          width: 18,
                          height: 18,
                          borderRadius: 9,
                          backgroundColor: "#FEE2E2",
                          display: "flex",
                          alignItems: "center",
                          justifyContent: "center",
                          fontSize: 10,
                          color: colors.danger,
                          fontWeight: 700,
                        }}
                      >
                        {i + 1}
                      </div>
                      <span style={{ fontSize: 12, color: "#888" }}>{step}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* Timer callout */}
          <div
            style={{
              opacity: ssrsCalloutOpacity,
              marginTop: 12,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              gap: 12,
            }}
          >
            <span style={{ fontSize: 18, fontWeight: 500, color: colors.textSecondary }}>
              <strong style={{ color: colors.textPrimary }}>SSRS</strong> — gathering exposure info before submitting:
            </span>
            {timerActive && (
              <span
                style={{
                  fontSize: 26,
                  fontWeight: 700,
                  color: colors.danger,
                  fontFamily: fonts.mono,
                }}
              >
                {timerMinutes}:{String(timerSec).padStart(2, "0")}+
              </span>
            )}
          </div>
        </div>
      </div>
    </AbsoluteFill>
  );
};
