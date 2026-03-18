import { useState, useEffect } from "react";
import { trainersApi, trainingsApi } from "../services/api";
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from "recharts";

const COLORS = ["#6c5ce7", "#34d399", "#60a5fa", "#fb923c", "#f472b6", "#a78bfa", "#fbbf24", "#ef4444"];
const tooltipStyle = { backgroundColor: "#18181c", border: "1px solid #2a2a30", borderRadius: "10px", color: "#eeeef0", fontSize: "13px", padding: "8px 12px" };

function Trainers() {
  const [trainers, setTrainers] = useState([]);
  const [trainings, setTrainings] = useState([]);
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [specialization, setSpecialization] = useState("");
  const [editingId, setEditingId] = useState(null);

  useEffect(() => { load(); }, []);

  async function load() {
    trainersApi.getAll().then(setTrainers).catch(() => {});
    trainingsApi.getAll().then(setTrainings).catch(() => {});
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const data = { firstName, lastName, specialization };
    if (editingId) {
      await trainersApi.update(editingId, { id: editingId, ...data });
      setEditingId(null);
    } else {
      await trainersApi.create(data);
    }
    setFirstName(""); setLastName(""); setSpecialization("");
    load();
  }

  function handleEdit(t) {
    setEditingId(t.id); setFirstName(t.firstName); setLastName(t.lastName); setSpecialization(t.specialization);
  }

  function handleCancelEdit() {
    setEditingId(null); setFirstName(""); setLastName(""); setSpecialization("");
  }

  async function handleDelete(id) { await trainersApi.delete(id); load(); }

  const specs = trainers.reduce((acc, t) => {
    const s = t.specialization || "Other";
    const ex = acc.find((x) => x.name === s);
    if (ex) ex.value += 1; else acc.push({ name: s, value: 1 });
    return acc;
  }, []);

  const uniqueSpecs = [...new Set(trainers.map((t) => t.specialization).filter(Boolean))];
  const busiest = trainers.reduce((best, t) => {
    const c = trainings.filter((tr) => tr.trainerId === t.id).length;
    return c > (best.count || 0) ? { name: `${t.firstName} ${t.lastName}`, count: c } : best;
  }, { name: "—", count: 0 });

  return (
    <div className="entity-section">
      <div className="dashboard-stats">
        <div className="stat-card purple">
          <div className="stat-header"><span className="stat-label">Total Trainers</span><div className="stat-icon-wrap purple">⚡</div></div>
          <div className="stat-value">{trainers.length}</div>
          <div className="stat-footer">Registered in system</div>
        </div>
        <div className="stat-card green">
          <div className="stat-header"><span className="stat-label">Specializations</span><div className="stat-icon-wrap green">🎯</div></div>
          <div className="stat-value">{uniqueSpecs.length}</div>
          <div className="stat-footer">Unique disciplines</div>
        </div>
        <div className="stat-card blue">
          <div className="stat-header"><span className="stat-label">Assigned Trainings</span><div className="stat-icon-wrap blue">🏋️</div></div>
          <div className="stat-value">{trainings.length}</div>
          <div className="stat-footer">Across all trainers</div>
        </div>
        <div className="stat-card orange">
          <div className="stat-header"><span className="stat-label">Top Trainer</span><div className="stat-icon-wrap orange">🏆</div></div>
          <div className="stat-value" style={{ fontSize: "20px" }}>{busiest.name}</div>
          <div className="stat-footer"><span className="stat-trend">{busiest.count}</span> trainings</div>
        </div>
      </div>

      <div className="page-grid">
        <div className="page-grid-main">
          <div className="section-header">
            <span className="section-title">Manage Trainers</span>
            <span className="record-count">{trainers.length} records</span>
          </div>
          <form className="entity-form" onSubmit={handleSubmit}>
            <div className="form-group"><label className="form-label">First Name</label><input className="form-input" placeholder="John" value={firstName} onChange={(e) => setFirstName(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Last Name</label><input className="form-input" placeholder="Doe" value={lastName} onChange={(e) => setLastName(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Specialization</label><input className="form-input" placeholder="Strength Training" value={specialization} onChange={(e) => setSpecialization(e.target.value)} required /></div>
            <div className="form-actions">
              <button type="submit" className="btn btn-primary">{editingId ? "✓ Update" : "+ Add"}</button>
              {editingId && <button type="button" className="btn btn-ghost" onClick={handleCancelEdit}>Cancel</button>}
            </div>
          </form>
          {trainers.length === 0 ? (
            <div className="table-wrapper"><div className="empty-state"><div className="empty-icon">⚡</div><div className="empty-text">No trainers yet</div><div className="empty-subtext">Add your first trainer above</div></div></div>
          ) : (
            <div className="table-wrapper">
              <table className="data-table">
                <thead><tr><th>ID</th><th>Trainer</th><th>Specialization</th><th>Trainings</th><th>Actions</th></tr></thead>
                <tbody>
                  {trainers.map((t) => {
                    const c = trainings.filter((tr) => tr.trainerId === t.id).length;
                    return (
                      <tr key={t.id}>
                        <td><span className="cell-id">#{t.id}</span></td>
                        <td><div className="cell-user"><div className="cell-avatar">{t.firstName[0]}{t.lastName[0]}</div><div><div className="cell-name">{t.firstName} {t.lastName}</div><div className="cell-sub">Trainer</div></div></div></td>
                        <td><span className="cell-badge">{t.specialization}</span></td>
                        <td><span className="cell-badge green">{c} assigned</span></td>
                        <td><div className="cell-actions"><button className="btn-icon" onClick={() => handleEdit(t)} title="Edit">✏️</button><button className="btn-icon danger" onClick={() => handleDelete(t.id)} title="Delete">🗑️</button></div></td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
        <div className="page-grid-side">
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot purple"></div> By Specialization</div>
            {specs.length > 0 ? (
              <ResponsiveContainer width="100%" height={180}>
                <PieChart><Pie data={specs} cx="50%" cy="50%" innerRadius={40} outerRadius={70} paddingAngle={4} dataKey="value" stroke="none">{specs.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}</Pie><Tooltip contentStyle={tooltipStyle} /></PieChart>
              </ResponsiveContainer>
            ) : (<div className="empty-state" style={{ padding: "24px" }}><div className="empty-subtext">Add trainers to see chart</div></div>)}
            <div className="legend-list">{specs.map((s, i) => (<div key={i} className="legend-item"><span className="legend-dot" style={{ background: COLORS[i % COLORS.length] }}></span><span className="legend-label">{s.name}</span><span className="legend-value">{s.value}</span></div>))}</div>
          </div>
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot green"></div> Quick Stats</div>
            <div className="info-list">
              <div className="info-row"><span className="info-icon">👤</span><span className="info-label">Latest</span><span className="info-value">{trainers.length > 0 ? `${trainers[trainers.length - 1].firstName} ${trainers[trainers.length - 1].lastName}` : "—"}</span></div>
              <div className="info-row"><span className="info-icon">📊</span><span className="info-label">Avg Trainings</span><span className="info-value">{trainers.length > 0 ? (trainings.length / trainers.length).toFixed(1) : "0"}</span></div>
              <div className="info-row"><span className="info-icon">🎯</span><span className="info-label">Top Spec</span><span className="info-value">{specs.length > 0 ? specs.sort((a, b) => b.value - a.value)[0].name : "—"}</span></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Trainers;
