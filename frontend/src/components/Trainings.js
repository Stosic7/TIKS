import { useState, useEffect } from "react";
import { trainingsApi, trainersApi } from "../services/api";
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, ResponsiveContainer, Tooltip } from "recharts";

const tooltipStyle = { backgroundColor: "#18181c", border: "1px solid #2a2a30", borderRadius: "10px", color: "#eeeef0", fontSize: "13px", padding: "8px 12px" };

function Trainings() {
  const [trainings, setTrainings] = useState([]);
  const [trainers, setTrainers] = useState([]);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [durationInMinutes, setDurationInMinutes] = useState("");
  const [trainerId, setTrainerId] = useState("");
  const [editingId, setEditingId] = useState(null);

  useEffect(() => { load(); }, []);

  async function load() {
    trainingsApi.getAll().then(setTrainings).catch(() => {});
    trainersApi.getAll().then(setTrainers).catch(() => {});
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const data = { name, description, durationInMinutes: parseInt(durationInMinutes), trainerId: parseInt(trainerId) };
    if (editingId) {
      await trainingsApi.update(editingId, { id: editingId, ...data });
      setEditingId(null);
    } else {
      await trainingsApi.create(data);
    }
    setName(""); setDescription(""); setDurationInMinutes(""); setTrainerId("");
    load();
  }

  function handleEdit(t) {
    setEditingId(t.id); setName(t.name); setDescription(t.description);
    setDurationInMinutes(t.durationInMinutes.toString()); setTrainerId(t.trainerId.toString());
  }

  function handleCancelEdit() {
    setEditingId(null); setName(""); setDescription(""); setDurationInMinutes(""); setTrainerId("");
  }

  async function handleDelete(id) { await trainingsApi.delete(id); load(); }

  const totalMin = trainings.reduce((s, t) => s + t.durationInMinutes, 0);
  const avgMin = trainings.length > 0 ? Math.round(totalMin / trainings.length) : 0;
  const longest = trainings.reduce((best, t) => t.durationInMinutes > (best.dur || 0) ? { name: t.name, dur: t.durationInMinutes } : best, { name: "—", dur: 0 });
  const shortest = trainings.length > 0 ? trainings.reduce((best, t) => t.durationInMinutes < best.dur ? { name: t.name, dur: t.durationInMinutes } : best, { name: trainings[0].name, dur: trainings[0].durationInMinutes }) : { name: "—", dur: 0 };

  const durationChart = trainings.map((t) => ({ name: t.name.length > 14 ? t.name.substring(0, 14) + "…" : t.name, duration: t.durationInMinutes }));
  const trainerLoad = trainers.map((tr) => ({ name: `${tr.firstName} ${tr.lastName[0]}.`, count: trainings.filter((t) => t.trainerId === tr.id).length })).filter((t) => t.count > 0);

  return (
    <div className="entity-section">
      <div className="dashboard-stats">
        <div className="stat-card purple">
          <div className="stat-header"><span className="stat-label">Total Trainings</span><div className="stat-icon-wrap purple">🏋️</div></div>
          <div className="stat-value">{trainings.length}</div>
          <div className="stat-footer">Available sessions</div>
        </div>
        <div className="stat-card green">
          <div className="stat-header"><span className="stat-label">Total Duration</span><div className="stat-icon-wrap green">⏱️</div></div>
          <div className="stat-value">{totalMin}<span style={{ fontSize: "14px", color: "var(--text-muted)", marginLeft: "4px" }}>min</span></div>
          <div className="stat-footer">Combined time</div>
        </div>
        <div className="stat-card blue">
          <div className="stat-header"><span className="stat-label">Avg Duration</span><div className="stat-icon-wrap blue">📐</div></div>
          <div className="stat-value">{avgMin}<span style={{ fontSize: "14px", color: "var(--text-muted)", marginLeft: "4px" }}>min</span></div>
          <div className="stat-footer">Per session</div>
        </div>
        <div className="stat-card orange">
          <div className="stat-header"><span className="stat-label">Longest Session</span><div className="stat-icon-wrap orange">🔥</div></div>
          <div className="stat-value" style={{ fontSize: "20px" }}>{longest.name}</div>
          <div className="stat-footer"><span className="stat-trend">{longest.dur}</span> minutes</div>
        </div>
      </div>

      <div className="page-grid">
        <div className="page-grid-main">
          <div className="section-header"><span className="section-title">Manage Trainings</span><span className="record-count">{trainings.length} records</span></div>
          <form className="entity-form" onSubmit={handleSubmit}>
            <div className="form-group"><label className="form-label">Name</label><input className="form-input" placeholder="HIIT Session" value={name} onChange={(e) => setName(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Description</label><input className="form-input" placeholder="High intensity interval" value={description} onChange={(e) => setDescription(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Duration (min)</label><input className="form-input" type="number" placeholder="45" value={durationInMinutes} onChange={(e) => setDurationInMinutes(e.target.value)} required /></div>
            <div className="form-group"><label className="form-label">Trainer</label>
              <select className="form-select" value={trainerId} onChange={(e) => setTrainerId(e.target.value)} required>
                <option value="">Select trainer...</option>
                {trainers.map((t) => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
              </select>
            </div>
            <div className="form-actions">
              <button type="submit" className="btn btn-primary">{editingId ? "✓ Update" : "+ Add"}</button>
              {editingId && <button type="button" className="btn btn-ghost" onClick={handleCancelEdit}>Cancel</button>}
            </div>
          </form>
          {trainings.length === 0 ? (
            <div className="table-wrapper"><div className="empty-state"><div className="empty-icon">🏋️</div><div className="empty-text">No trainings yet</div><div className="empty-subtext">Add your first training above</div></div></div>
          ) : (
            <div className="table-wrapper">
              <table className="data-table">
                <thead><tr><th>ID</th><th>Training</th><th>Duration</th><th>Trainer</th><th>Actions</th></tr></thead>
                <tbody>
                  {trainings.map((t) => (
                    <tr key={t.id}>
                      <td><span className="cell-id">#{t.id}</span></td>
                      <td><div className="cell-user"><div className="cell-avatar-square">🏋️</div><div><div className="cell-name">{t.name}</div><div className="cell-sub">{t.description}</div></div></div></td>
                      <td><span className="cell-badge green">{t.durationInMinutes} min</span></td>
                      <td>{t.trainer ? <div className="cell-user"><div className="cell-avatar">{t.trainer.firstName[0]}{t.trainer.lastName[0]}</div><span className="cell-name">{t.trainer.firstName} {t.trainer.lastName}</span></div> : t.trainerId}</td>
                      <td><div className="cell-actions"><button className="btn-icon" onClick={() => handleEdit(t)} title="Edit">✏️</button><button className="btn-icon danger" onClick={() => handleDelete(t.id)} title="Delete">🗑️</button></div></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
        <div className="page-grid-side">
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot blue"></div> Duration Breakdown</div>
            {durationChart.length > 0 ? (
              <ResponsiveContainer width="100%" height={200}>
                <BarChart data={durationChart} barSize={24}><CartesianGrid strokeDasharray="3 3" stroke="#1a1a1f" /><XAxis dataKey="name" tick={{ fill: "#505068", fontSize: 10 }} axisLine={false} tickLine={false} /><YAxis tick={{ fill: "#505068", fontSize: 11 }} axisLine={false} tickLine={false} /><Tooltip contentStyle={tooltipStyle} cursor={{ fill: "rgba(108,92,231,0.06)" }} /><Bar dataKey="duration" fill="#60a5fa" radius={[6,6,0,0]} /></BarChart>
              </ResponsiveContainer>
            ) : (<div className="empty-state" style={{ padding: "24px" }}><div className="empty-subtext">Add trainings to see chart</div></div>)}
          </div>
          <div className="side-card">
            <div className="chart-title"><div className="chart-title-dot orange"></div> Quick Stats</div>
            <div className="info-list">
              <div className="info-row"><span className="info-icon">⏱️</span><span className="info-label">Shortest</span><span className="info-value">{shortest.name} ({shortest.dur}m)</span></div>
              <div className="info-row"><span className="info-icon">🔥</span><span className="info-label">Longest</span><span className="info-value">{longest.name} ({longest.dur}m)</span></div>
              <div className="info-row"><span className="info-icon">📊</span><span className="info-label">Average</span><span className="info-value">{avgMin} min</span></div>
              <div className="info-row"><span className="info-icon">👥</span><span className="info-label">Trainers Used</span><span className="info-value">{trainerLoad.length}</span></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Trainings;
