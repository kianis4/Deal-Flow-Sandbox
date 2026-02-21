import { AbsoluteFill, interpolate, useCurrentFrame, spring, useVideoConfig } from "remotion";
import { colors, fonts } from "../styles";

/**
 * Scene 5: The exposure lookup — the hero feature.
 * "What used to take 15-20 minutes now takes <1 second."
 * Shows the animated search, stats cards, document tier, and deal table.
 */

const StatCard: React.FC<{
  label: string;
  value: string;
  color: string;
  opacity: number;
}> = ({ label, value, color, opacity }) => (
  <div
    style={{
      opacity,
      padding: "16px 24px",
      borderRadius: 14,
      backgroundColor: colors.bgWhite,
      border: `1.5px solid ${colors.border}`,
      boxShadow: "0 2px 12px rgba(0,0,0,0.04)",
      minWidth: 170,
      textAlign: "center",
    }}
  >
    <div style={{ fontSize: 34, fontWeight: 700, color, fontFamily: fonts.mono, lineHeight: 1 }}>
      {value}
    </div>
    <div style={{ fontSize: 13, color: colors.textMuted, marginTop: 6, fontWeight: 500 }}>
      {label}
    </div>
  </div>
);

export const ExposureLookupScene: React.FC = () => {
  const frame = useCurrentFrame();
  const { fps } = useVideoConfig();

  const headerOpacity = interpolate(frame, [0, 20], [0, 1], { extrapolateRight: "clamp" });

  // Search bar typing animation
  const searchOpacity = interpolate(frame, [20, 35], [0, 1], { extrapolateRight: "clamp" });
  const typingProgress = interpolate(frame, [45, 85], [0, 1], { extrapolateRight: "clamp" });
  const searchText = "TransCanada Hauling".slice(0, Math.floor(typingProgress * 19));
  const cursorVisible = frame >= 45 && frame < 95 && Math.floor(frame / 8) % 2 === 0;

  // Results flash in (like instant)
  const flashOpacity = interpolate(frame, [95, 100], [0, 1], { extrapolateRight: "clamp" });
  const statsOpacity = interpolate(frame, [100, 115], [0, 1], { extrapolateRight: "clamp" });
  const tierOpacity = interpolate(frame, [120, 135], [0, 1], { extrapolateRight: "clamp" });
  const tableOpacity = interpolate(frame, [140, 155], [0, 1], { extrapolateRight: "clamp" });

  // "That was instant" callout
  const calloutOpacity = interpolate(frame, [160, 180], [0, 1], { extrapolateRight: "clamp" });

  // Comparison at bottom
  const compareOpacity = interpolate(frame, [220, 250], [0, 1], { extrapolateRight: "clamp" });

  const deals = [
    { equipment: "Semi-Truck (Kenworth T680)", netInvest: "$195,000", status: "FUNDED", statusColor: colors.success, nsfs: 2, delinquent: false },
    { equipment: "Semi-Truck (Peterbilt 579)", netInvest: "$185,000", status: "FUNDED", statusColor: colors.success, nsfs: 1, delinquent: false },
    { equipment: "Dry Van Trailer (Utility 4000D-X)", netInvest: "$58,143", status: "FUNDED", statusColor: colors.success, nsfs: 0, delinquent: false },
    { equipment: "Reefer Trailer (Carrier X4 7500)", netInvest: "$97,300", status: "FUNDED", statusColor: colors.success, nsfs: 0, delinquent: true },
  ];

  return (
    <AbsoluteFill
      style={{
        background: colors.bgLight,
        fontFamily: fonts.heading,
        padding: "40px 70px",
      }}
    >
      <div style={{ opacity: headerOpacity, textAlign: "center", marginBottom: 8 }}>
        <p
          style={{
            fontSize: 16,
            fontWeight: 600,
            color: colors.primary,
            letterSpacing: 2,
            textTransform: "uppercase",
            margin: "0 0 6px 0",
          }}
        >
          The Feature That Saves Hours
        </p>
        <h2 style={{ fontSize: 40, fontWeight: 700, color: colors.textPrimary, margin: 0 }}>
          Party Exposure Lookup
        </h2>
      </div>

      {/* Search bar */}
      <div style={{ opacity: searchOpacity, display: "flex", justifyContent: "center", marginBottom: 20 }}>
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: 12,
            backgroundColor: colors.bgWhite,
            borderRadius: 12,
            border: `2px solid ${frame >= 95 ? colors.success : colors.primary}`,
            padding: "10px 20px",
            width: 580,
            boxShadow: `0 4px 20px ${frame >= 95 ? "rgba(16,124,16,0.12)" : "rgba(0,120,212,0.1)"}`,
          }}
        >
          <div
            style={{
              padding: "4px 12px",
              borderRadius: 8,
              backgroundColor: colors.primaryLight,
              fontSize: 13,
              fontWeight: 600,
              color: colors.primary,
            }}
          >
            Customer
          </div>
          <div
            style={{
              fontSize: 18,
              color: searchText ? colors.textPrimary : colors.textMuted,
              fontFamily: fonts.body,
              flex: 1,
            }}
          >
            {searchText || "Enter client or dealer name..."}
            {cursorVisible && <span style={{ borderRight: `2px solid ${colors.primary}` }}>&nbsp;</span>}
          </div>
          <div
            style={{
              padding: "6px 16px",
              borderRadius: 8,
              backgroundColor: frame >= 95 ? colors.success : colors.primary,
              color: "#FFF",
              fontSize: 14,
              fontWeight: 600,
            }}
          >
            {frame >= 95 ? "✓" : "Search"}
          </div>
        </div>
      </div>

      {/* Instant flash + stats */}
      <div style={{ display: "flex", justifyContent: "center", gap: 20, marginBottom: 16 }}>
        <StatCard label="Active Deals" value="4" color={colors.primary} opacity={statsOpacity} />
        <StatCard label="Net Exposure" value="$535K" color={colors.textPrimary} opacity={statsOpacity} />
        <StatCard label="Total NSFs" value="3" color={colors.warning} opacity={statsOpacity} />
        <StatCard label="Delinquent" value="1" color={colors.danger} opacity={statsOpacity} />
        <StatCard label="Last NSF" value="45d ago" color={colors.danger} opacity={statsOpacity} />
      </div>

      {/* Document tier banner */}
      <div style={{ opacity: tierOpacity, display: "flex", justifyContent: "center", marginBottom: 16 }}>
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: 10,
            padding: "8px 24px",
            borderRadius: 10,
            backgroundColor: "#FFF8E6",
            border: "1.5px solid #F7A600",
          }}
        >
          <div style={{ width: 10, height: 10, borderRadius: 5, backgroundColor: "#F7A600" }} />
          <span style={{ fontSize: 15, fontWeight: 600, color: "#8B5E00" }}>
            Enhanced Tier — Bank statements required (net exposure &gt; $250K)
          </span>
        </div>
      </div>

      {/* Deal table */}
      <div
        style={{
          opacity: tableOpacity,
          backgroundColor: colors.bgWhite,
          borderRadius: 14,
          border: `1.5px solid ${colors.border}`,
          overflow: "hidden",
          maxWidth: 950,
          margin: "0 auto 16px auto",
          boxShadow: "0 2px 12px rgba(0,0,0,0.04)",
        }}
      >
        {/* Header */}
        <div
          style={{
            display: "flex",
            padding: "10px 20px",
            backgroundColor: colors.bgLight,
            borderBottom: `1.5px solid ${colors.border}`,
            gap: 16,
            fontSize: 12,
            fontWeight: 600,
            color: colors.textMuted,
            textTransform: "uppercase",
            letterSpacing: 1,
          }}
        >
          <div style={{ flex: 2.5 }}>Equipment</div>
          <div style={{ flex: 1, textAlign: "right" }}>Net Invest</div>
          <div style={{ flex: 1, textAlign: "center" }}>Status</div>
          <div style={{ flex: 0.5, textAlign: "center" }}>NSFs</div>
          <div style={{ flex: 0.7, textAlign: "center" }}>Delinquent</div>
        </div>
        {deals.map((deal, i) => {
          const rowOpacity = interpolate(frame, [155 + i * 10, 165 + i * 10], [0, 1], { extrapolateRight: "clamp" });
          return (
            <div
              key={deal.equipment}
              style={{
                opacity: rowOpacity,
                display: "flex",
                padding: "10px 20px",
                borderBottom: i < deals.length - 1 ? `1px solid ${colors.borderLight}` : "none",
                gap: 16,
                alignItems: "center",
              }}
            >
              <div style={{ flex: 2.5, fontSize: 15, fontWeight: 500, color: colors.textPrimary }}>{deal.equipment}</div>
              <div style={{ flex: 1, fontSize: 15, fontFamily: fonts.mono, color: colors.textPrimary, textAlign: "right" }}>{deal.netInvest}</div>
              <div style={{ flex: 1, textAlign: "center" }}>
                <span
                  style={{
                    fontSize: 12,
                    fontWeight: 600,
                    color: deal.statusColor,
                    backgroundColor: deal.statusColor + "15",
                    padding: "2px 10px",
                    borderRadius: 10,
                  }}
                >
                  {deal.status}
                </span>
              </div>
              <div
                style={{
                  flex: 0.5,
                  textAlign: "center",
                  fontSize: 15,
                  fontWeight: deal.nsfs > 0 ? 700 : 400,
                  color: deal.nsfs > 0 ? colors.warning : colors.textMuted,
                }}
              >
                {deal.nsfs}
              </div>
              <div
                style={{
                  flex: 0.7,
                  textAlign: "center",
                  fontSize: 12,
                  fontWeight: 600,
                  color: deal.delinquent ? colors.danger : colors.success,
                }}
              >
                {deal.delinquent ? "45 days" : "—"}
              </div>
            </div>
          );
        })}
      </div>

      {/* "That was instant" callout */}
      <div style={{ opacity: calloutOpacity, textAlign: "center", marginBottom: 10 }}>
        <span
          style={{
            fontSize: 18,
            fontWeight: 600,
            color: colors.success,
            padding: "6px 20px",
            borderRadius: 20,
            backgroundColor: "#E6F4E6",
          }}
        >
          Total exposure, NSFs, delinquency, document tier — all in under 1 second
        </span>
      </div>

      {/* Before/After comparison */}
      <div
        style={{
          opacity: compareOpacity,
          display: "flex",
          justifyContent: "center",
          gap: 60,
          marginTop: 6,
        }}
      >
        <div style={{ textAlign: "center" }}>
          <div style={{ fontSize: 13, color: colors.textMuted, textTransform: "uppercase", letterSpacing: 1, fontWeight: 600, marginBottom: 4 }}>Before</div>
          <div style={{ fontSize: 28, fontWeight: 700, color: colors.danger, fontFamily: fonts.mono }}>15–20 min</div>
          <div style={{ fontSize: 13, color: colors.textMuted }}>SSRS + manual tally per client</div>
        </div>
        <div style={{ display: "flex", alignItems: "center", fontSize: 28, color: colors.primary }}>→</div>
        <div style={{ textAlign: "center" }}>
          <div style={{ fontSize: 13, color: colors.textMuted, textTransform: "uppercase", letterSpacing: 1, fontWeight: 600, marginBottom: 4 }}>After</div>
          <div style={{ fontSize: 28, fontWeight: 700, color: colors.success, fontFamily: fonts.mono }}>{"<"}1s</div>
          <div style={{ fontSize: 13, color: colors.textMuted }}>Instant API + auto thresholds</div>
        </div>
      </div>
    </AbsoluteFill>
  );
};
