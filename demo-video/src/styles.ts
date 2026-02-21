import { CSSProperties } from "react";

// Corporate color palette
export const colors = {
  // Primary blues (Azure-inspired)
  primary: "#0078D4",
  primaryDark: "#005A9E",
  primaryLight: "#DEECF9",

  // Backgrounds
  bgWhite: "#FFFFFF",
  bgLight: "#F5F7FA",
  bgDark: "#1B1B1F",
  bgCard: "#FFFFFF",

  // Text
  textPrimary: "#1B1B1F",
  textSecondary: "#5C5C70",
  textMuted: "#8E8E9A",
  textOnDark: "#FFFFFF",

  // Accent colors
  success: "#107C10",
  warning: "#F7630C",
  danger: "#D13438",
  info: "#0078D4",

  // Service colors
  intakeApi: "#0078D4",
  scoringWorker: "#8661C5",
  notifyWorker: "#107C10",
  reportingApi: "#D83B01",

  // Borders
  border: "#E1E1E8",
  borderLight: "#F0F0F5",
};

export const fonts = {
  heading: "'Segoe UI', -apple-system, BlinkMacSystemFont, sans-serif",
  body: "'Segoe UI', -apple-system, BlinkMacSystemFont, sans-serif",
  mono: "'Cascadia Code', 'Fira Code', monospace",
};

// Reusable styles
export const containerStyle: CSSProperties = {
  width: "100%",
  height: "100%",
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  justifyContent: "center",
  fontFamily: fonts.heading,
  backgroundColor: colors.bgLight,
  padding: "60px 80px",
  boxSizing: "border-box",
};

export const headingStyle: CSSProperties = {
  fontSize: 56,
  fontWeight: 700,
  color: colors.textPrimary,
  margin: 0,
  lineHeight: 1.2,
};

export const subheadingStyle: CSSProperties = {
  fontSize: 28,
  fontWeight: 400,
  color: colors.textSecondary,
  margin: 0,
  lineHeight: 1.5,
};
