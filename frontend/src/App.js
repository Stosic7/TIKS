import { useState } from "react";
import Trainers from "./components/Trainers";
import Trainings from "./components/Trainings";
import Members from "./components/Members";
import TrainingPlans from "./components/TrainingPlans";
import Dashboard from "./components/Dashboard";

const tabs = [
  { key: "dashboard", label: "Dashboard", icon: "📊" },
  { key: "trainers", label: "Trainers", icon: "⚡" },
  { key: "trainings", label: "Trainings", icon: "🏋️" },
  { key: "members", label: "Members", icon: "👥" },
  { key: "plans", label: "Plans", icon: "📋" },
];

function App() {
  const [activeTab, setActiveTab] = useState("dashboard");

  return (
    <div className="app">
      <div className="animated-bg">
        <div className="orb orb-1"></div>
        <div className="orb orb-2"></div>
        <div className="orb orb-3"></div>
      </div>
      <div className="grid-overlay"></div>

      <aside className="sidebar">
        <div className="sidebar-logo">
          <div className="logo-mark">G</div>
          <span className="logo-title">GymOS</span>
        </div>
        <div className="sidebar-divider"></div>
        <span className="nav-section-label">Navigation</span>
        <nav className="sidebar-nav">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              className={`nav-item ${activeTab === tab.key ? "active" : ""}`}
              onClick={() => setActiveTab(tab.key)}
            >
              <span className="nav-icon">{tab.icon}</span>
              <span className="nav-label">{tab.label}</span>
            </button>
          ))}
        </nav>
        <div className="sidebar-footer">
          <div className="status-indicator"></div>
          <span className="status-text">Status: <strong>Online</strong></span>
        </div>
      </aside>

      <main className="main-content">
        <header className="top-bar">
          <h1 className="page-title">
            {tabs.find((t) => t.key === activeTab)?.icon}{" "}
            {tabs.find((t) => t.key === activeTab)?.label}
          </h1>
          <div className="top-bar-right">
            <span className="top-badge">GymOS v1.0</span>
          </div>
        </header>
        <div className="content-area" key={activeTab}>
          {activeTab === "dashboard" && <Dashboard />}
          {activeTab === "trainers" && <Trainers />}
          {activeTab === "trainings" && <Trainings />}
          {activeTab === "members" && <Members />}
          {activeTab === "plans" && <TrainingPlans />}
        </div>
      </main>
    </div>
  );
}

export default App;
