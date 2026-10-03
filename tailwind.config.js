const defaultTheme = require("tailwindcss/defaultTheme");

/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "class",
  content: ["./Views/**/*.cshtml", "./wwwroot/js/**/*.js"],
  theme: {
    screens: {
      ...defaultTheme.screens,
      "max-lg": { max: "1023px" },
    },
    extend: {
      colors: {
        bg: "var(--bg)",
        panel: "var(--panel)",
        ink: "var(--ink)",
        ink2: "var(--ink2)",
        muted: "var(--muted)",
        line: "var(--line)",
        chip: "var(--chip)",
        nav: "var(--nav)",
        navink: "var(--navink)",
        navactive: "var(--navactive)",
        save: "var(--save)",
        saveSoft: "var(--saveSoft)",
        debt: "var(--debt)",
        debtSoft: "var(--debtSoft)",
        danger: "var(--danger)",
      },
      fontFamily: {
        sans: ["var(--font-family)", "sans-serif"],
      },
      borderRadius: {
        card: "14px",
        chip: "10px",
      },
      boxShadow: {
        card: "0 1px 2px rgba(15,23,42,.06), 0 10px 30px rgba(15,23,42,.06)",
      },
    },
  },
  plugins: [],
};
