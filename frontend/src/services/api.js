const BASE_URL = "http://localhost:5229/api";

async function request(endpoint, options = {}) {
  const response = await fetch(`${BASE_URL}${endpoint}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });
  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`);
  }
  if (response.status === 204) return null;
  return response.json();
}

export const trainersApi = {
  getAll: () => request("/Trainers"),
  getById: (id) => request(`/Trainers/${id}`),
  create: (data) => request("/Trainers", { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => request(`/Trainers/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  delete: (id) => request(`/Trainers/${id}`, { method: "DELETE" }),
};

export const trainingsApi = {
  getAll: () => request("/Trainings"),
  getById: (id) => request(`/Trainings/${id}`),
  create: (data) => request("/Trainings", { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => request(`/Trainings/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  delete: (id) => request(`/Trainings/${id}`, { method: "DELETE" }),
};

export const membersApi = {
  getAll: () => request("/Members"),
  getById: (id) => request(`/Members/${id}`),
  create: (data) => request("/Members", { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => request(`/Members/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  delete: (id) => request(`/Members/${id}`, { method: "DELETE" }),
};

export const trainingPlansApi = {
  getAll: () => request("/TrainingPlans"),
  getById: (id) => request(`/TrainingPlans/${id}`),
  create: (data) => request("/TrainingPlans", { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => request(`/TrainingPlans/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  delete: (id) => request(`/TrainingPlans/${id}`, { method: "DELETE" }),
};
